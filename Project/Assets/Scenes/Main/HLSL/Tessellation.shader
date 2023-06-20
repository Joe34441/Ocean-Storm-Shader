Shader "Custom/Tessellation"
{
    Properties
    {
        _Color("Colour", Color) = (1, 0.6, 1, 0.05)
        _DrawCracks("Draw Cracks", Integer) = 0 
        _FactorEdge("Edge factors", Vector) = (1, 1, 1, 0)
        _FactorInside("Inside factor", Float) = 1
        _TessellationFactor("Tessellation Factor", Float) = 1
        _Tolerance("Tolerance", Float) = 0

        _Amount("Amount", Integer) = 1
        _SourcePosition("Source Position", Vector) = (0.1, 0.1, 0.1, 0)
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back
        LOD 200

        Pass
        {
            HLSLPROGRAM
            #pragma target 5.0
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma geometry Geometry //
            #pragma hull Hull
            #pragma domain Domain
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct TessellationFactors
            {
                float edge[3] : SV_TessFactor;
                float inside : SV_InsideTessFactor;
            };

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct TessellationControlPoint
            {
                float3 positionWS : INTERNALTESSPOS;
                float3 normalWS : NORMAL;
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Interpolators
            {
                float3 normalWS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct g2f
            {
                float4 pos : SV_POSITION;
                float3 barycentric : TEXTCOORD0;
            };

            #define BARYCENTRIC_INTERPOLATE(fieldName) \
            patch[0].fieldName * barycentricCoordinates.x + \
            patch[1].fieldName * barycentricCoordinates.y + \
            patch[2].fieldName * barycentricCoordinates.z

            bool IsOutOfBounds(float3 p, float3 lower, float3 higher)
            {
                return p.x < lower.x || p.x > higher.x ||
                    p.y < lower.y || p.y > higher.y ||
                    p.z < lower.z || p.z > higher.z;
            }

            bool IsPointOutOfFrustum(float4 positionCS, float tolerance)
            {
                float3 culling = positionCS.xyz;
                float w = positionCS.w;
                //UNITY_RAW_FAR_CLIP_VALUE is either 0 or 1, depending on graphics API
                //Most use 0, howerver OpenGL uses 1
                //this should be vecotrs that we set through c# using mouse click position on mesh, bounding the area we tessellate in
                float3 lowerBounds = float3(-w - tolerance, -w - tolerance, -w * UNITY_RAW_FAR_CLIP_VALUE - tolerance);
                float3 higherBounds = float3(w + tolerance, w + tolerance, w + tolerance);

                return IsOutOfBounds(culling, lowerBounds, higherBounds);
            }

            bool ShouldBackFaceCull(float4 p0PositionCS, float4 p1PositionCS, float4 p2PositionCS, float tolerance)
            {
                float3 point0 = p0PositionCS.xyz / p0PositionCS.w;
                float3 point1 = p1PositionCS.xyz / p1PositionCS.w;
                float3 point2 = p2PositionCS.xyz / p2PositionCS.w;

                #if !UNITY_REVERSE_Z
                    return cross(point1 - point0, point2 - point0).z < -tolerance;
                #else //in OpenGL, the test is reversed
                    return cross(point1 - point0, point2 - point0).z > tolerance;
                #endif
            }

            bool ShouldClipPatch(float4 p0PositionCS, float4 p1PositionCS, float4 p2PositionCS, float tolerance)
            {
                bool allOutside = IsPointOutOfFrustum(p0PositionCS, tolerance) && IsPointOutOfFrustum(p1PositionCS, tolerance) && IsPointOutOfFrustum(p2PositionCS, tolerance);
                return allOutside || ShouldBackFaceCull(p0PositionCS, p1PositionCS, p2PositionCS, tolerance);
            }

            float4 _SourcePosition;

            float EdgeTessellationFactor(float scale, float3 p0PositionWS, float3 p0PositionCS, float3 p1PositionWS, float3 p1PositionCS)
            {
                float3 mousePos = float3(_SourcePosition.x, _SourcePosition.y, _SourcePosition.z);
                float length = distance(p0PositionWS, p1PositionWS);
                float distanceToMouseClick = distance(mousePos, (p0PositionWS + p1PositionWS) * 0.5);
                float factor = length / (scale * distanceToMouseClick * distanceToMouseClick);

                float spreadFactor = factor / 15;

                if (spreadFactor > 20) spreadFactor = 20;

                return max(1, spreadFactor);
            }

            float4 _FactorEdge;
            float _FactorInside;
            float _Tolerance;
            float _TessellationFactor;
            float _Amount;

            //the patch constant function runs once per triangle, or "patch"
            //it runs in parallel to the hull function
            TessellationFactors PatchConstantFunction(InputPatch<TessellationControlPoint, 3> patch)
            {
                //setup instancing
                UNITY_SETUP_INSTANCE_ID(patch[0]);

                //calculate tessellation factors
                TessellationFactors f = (TessellationFactors)0;

                //check if this patch should be culled (it is out of view)
                if (ShouldClipPatch(patch[0].positionCS, patch[1].positionCS, patch[2].positionCS, _Tolerance))
                {
                    //cull the patch
                    f.edge[0] = 0;
                    f.edge[1] = 0;
                    f.edge[2] = 0;
                    f.inside = 0;
                }
                else
                {
                    float edge1 = EdgeTessellationFactor(_TessellationFactor, patch[1].positionWS, patch[1].positionCS, patch[2].positionWS, patch[2].positionCS);
                    float edge2 = EdgeTessellationFactor(_TessellationFactor, patch[2].positionWS, patch[2].positionCS, patch[0].positionWS, patch[0].positionCS);
                    float edge3 = EdgeTessellationFactor(_TessellationFactor, patch[0].positionWS, patch[0].positionCS, patch[1].positionWS, patch[1].positionCS);

                    f.edge[0] = edge1;
                    f.edge[1] = edge2;
                    f.edge[2] = edge3;

                    f.inside = (edge1 + edge2 + edge3) / 3.0;
                }
                return f;
            }

            //the hull function runs once per vertex. You can use it to modify vertex data based on values in the entire triangle
            [domain("tri")] //signal we're inputting triangles
            [outputcontrolpoints(3)] //triangles have three points
            [outputtopology("triangle_cw")] //signal we're outputting triangles
            [patchconstantfunc("PatchConstantFunction")] //register the patch constant function
            [partitioning("integer")] //select a partitioning mode: integer, fractional_odd, fractional_even or pow2
            TessellationControlPoint Hull(InputPatch<TessellationControlPoint, 3> patch, uint id : SV_OutputControlPointID)
            {
                return patch[id];
            }

            //the domain function runs once per vertex in the final, tessellated mesh
            //use it to reposition vertices and prepare for the fragment stage
            [domain("tri")] //signal we're inputting triangles
            Interpolators Domain(TessellationFactors factors, OutputPatch <TessellationControlPoint, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
            {
                Interpolators output;

                float3 positionWS = BARYCENTRIC_INTERPOLATE(positionWS);
                float3 normalWS = BARYCENTRIC_INTERPOLATE(normalWS);

                output.positionCS = TransformWorldToHClip(positionWS);
                output.normalWS = normalWS;
                output.positionWS = positionWS;

                return output;
            }

            TessellationControlPoint Vertex(Attributes input)
            {
                TessellationControlPoint output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs posnInputs = GetVertexPositionInputs(input.positionOS);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                output.positionWS = posnInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.positionCS = posnInputs.positionCS;

                return output;
            }

            float4 _Color;
            int _DrawCracks;

            float4 Fragment(g2f i) : SV_Target
            {
                if (_DrawCracks == 0) return float4(0, 0, 0, 0);

                //find the barycentric coordinate closest to the edge
                float closest = min(i.barycentric.x, min(i.barycentric.y, i.barycentric.z));
                //get new alpha
                float alpha = step(closest, 0.01);

                return float4(0.5, 0.5, 0.5, alpha);
            }

            [maxvertexcount(3)]
            void Geometry(triangle Interpolators i[3], inout TriangleStream<g2f> stream)
            {
                g2f o;
                o.pos = i[0].positionCS;
                o.barycentric = float3(1.0, 0.0, 0.0);
                stream.Append(o);
                o.pos = i[1].positionCS;
                o.barycentric = float3(0.0, 1.0, 0.0);
                stream.Append(o);
                o.pos = i[2].positionCS;
                o.barycentric = float3(0.0, 0.0, 1.0);
                stream.Append(o);
            }

            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
