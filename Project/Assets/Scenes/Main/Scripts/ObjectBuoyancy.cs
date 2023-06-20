using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
public class ObjectBuoyancy : MonoBehaviour
{
    private Vector3 result;

    private GameObject waterObject;
    private Material waterMaterial;

    private Vector3 waveHeight1;
    private Vector3 waveHeight2;
    private Vector3 waveHeight3;
    private Vector3 waveHeight4;

    private MeshRenderer meshRenderer;
    private Rigidbody rigidBody;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        rigidBody = GetComponent<Rigidbody>();
        if (rigidBody == null)
        {
            rigidBody = gameObject.AddComponent<Rigidbody>();
            rigidBody.mass = 1;
            rigidBody.drag = 2;
            rigidBody.angularDrag = 8;
            rigidBody.useGravity = true;
            rigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        waterObject = GameObject.FindGameObjectWithTag("Water");
        waterMaterial = waterObject.GetComponent<MeshRenderer>().material;

        waveHeight1 = Vector3.zero;
        waveHeight2 = Vector3.zero;
        waveHeight3 = Vector3.zero;
        waveHeight4 = Vector3.zero;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        waterMaterial.SetFloat("_Running", 1);
        waterMaterial.SetFloat("_TimeMultiplier", Time.timeSinceLevelLoad);

        Vector3[] directions = new Vector3[4];
        directions[0] = waterMaterial.GetVector("_Direction1");
        directions[1] = waterMaterial.GetVector("_Direction2");
        directions[2] = waterMaterial.GetVector("_Direction3");
        directions[3] = waterMaterial.GetVector("_Direction4");

        float gravity = waterMaterial.GetFloat("_Gravity");

        float depth = waterMaterial.GetFloat("_Depth");

        float[] times = new float[4];
        times[0] = waterMaterial.GetVector("_Timescales").x;
        times[1] = waterMaterial.GetVector("_Timescales").y;
        times[2] = waterMaterial.GetVector("_Timescales").z;
        times[3] = waterMaterial.GetVector("_Timescales").w;

        float phase = waterMaterial.GetFloat("_Phase");

        float[] amplitudes = new float[4];
        amplitudes[0] = waterMaterial.GetFloat("_Amplitude1");
        amplitudes[1] = waterMaterial.GetFloat("_Amplitude2");
        amplitudes[2] = waterMaterial.GetFloat("_Amplitude3");
        amplitudes[3] = waterMaterial.GetFloat("_Amplitude4");


        //get mesh face location
        Vector3 position = GetPos(1);

        result = GetWaveDisplacement(position, directions, gravity, depth, times, phase, amplitudes);

        if (float.IsNaN(result.x))
        {
            Debug.Log("nan value");
            return;
        }

        Vector3 newpos = result;
        newpos.x = position.x;
        newpos.z = position.z;
        newpos += waterObject.transform.position;
        waveHeight1 = newpos;

        position = GetPos(2);

        result = GetWaveDisplacement(position, directions, gravity, depth, times, phase, amplitudes);

        if (float.IsNaN(result.x))
        {
            Debug.Log("nan value");
            return;
        }

        newpos = result;
        newpos.x = position.x;
        newpos.z = position.z;
        newpos += waterObject.transform.position;
        waveHeight2 = newpos;

        position = GetPos(3);

        result = GetWaveDisplacement(position, directions, gravity, depth, times, phase, amplitudes);

        if (float.IsNaN(result.x))
        {
            Debug.Log("nan value");
            return;
        }

        newpos = result;
        newpos.x = position.x;//transform.position.x;
        newpos.z = position.z;//transform.position.z;
        newpos += waterObject.transform.position;
        waveHeight3 = newpos;

        //waveHeight3.transform.position = result;
        //waveHeight3.transform.position += waterObject.transform.position;

        //extra = transform.position;
        //extra.y = 0;

        //waveHeight3.transform.position += extra;



        position = GetPos(4);

        result = GetWaveDisplacement(position, directions, gravity, depth, times, phase, amplitudes);

        if (float.IsNaN(result.x))
        {
            Debug.Log("nan value");
            return;
        }

        newpos = result;
        newpos.x = position.x;//transform.position.x;
        newpos.z = position.z;//transform.position.z;
        newpos += waterObject.transform.position;
        waveHeight4 = newpos;

        //waveHeight4.transform.position = result;
        //waveHeight4.transform.position += waterObject.transform.position;

        //extra = transform.position;
        //extra.y = 0;

        //waveHeight4.transform.position += extra;



        //apply relevant force to mesh face towards wave height
        ApplyForces(directions);

        return;
    }

    public Vector3 GetPos(int side)
    {
        Vector3 position = transform.position;
        position.y = waterObject.transform.position.y;

        if (side == 1)
        {
            position.x -= meshRenderer.bounds.extents.x;
        }
        else if (side == 2)
        {
            position.z -= meshRenderer.bounds.extents.z;
        }
        else if (side == 3)
        {
            position.x += meshRenderer.bounds.extents.x;
        }
        else if (side == 4)
        {
            position.z += meshRenderer.bounds.extents.z;
        }

        return position;
    }

    public void ApplyForces(Vector3[] directions)
    {
        Vector3 newF = Vector3.zero;
        Vector3 newPos;

        float forceMulti = 2;

        newF.y = waveHeight1.y - transform.position.y;
        newPos = transform.position;
        newPos.x -= meshRenderer.bounds.extents.x / 2;

        rigidBody.AddForceAtPosition(newF / forceMulti, newPos);


        newF.y = waveHeight2.y - transform.position.y;
        newPos = transform.position;
        newPos.z -= meshRenderer.bounds.extents.z / 2;

        rigidBody.AddForceAtPosition(newF / forceMulti, newPos);


        newF.y = waveHeight3.y - transform.position.y;
        newPos = transform.position;
        newPos.x += meshRenderer.bounds.extents.x / 2;

        rigidBody.AddForceAtPosition(newF / forceMulti, newPos);


        newF.y = waveHeight4.y - transform.position.y;
        newPos = transform.position;
        newPos.z += meshRenderer.bounds.extents.z / 2;

        rigidBody.AddForceAtPosition(newF / forceMulti, newPos);


        Vector3 endForce = directions[0] + directions[1] + directions[2] + directions[3];
        rigidBody.AddForce(endForce / 10, ForceMode.Impulse);
    }

    public Vector3 GetWaveDisplacement(Vector3 position, Vector3[] directions, float gravity, float depth, float[] times, float phase, float[] amplitudes)
    {
        Vector3 offset = Vector3.zero;

        offset += GerstnerWave(position, directions[0], gravity, depth, times[0], phase, amplitudes[0]);
        offset += GerstnerWave(position, directions[1], gravity, depth, times[1], phase, amplitudes[1]);
        offset += GerstnerWave(position, directions[2], gravity, depth, times[2], phase, amplitudes[2]);
        offset += GerstnerWave(position, directions[3], gravity, depth, times[3], phase, amplitudes[3]);

        return offset;
    }

    private Vector3 GerstnerWave(Vector3 position, Vector3 direction, float gravity, float depth,float time, float phase, float amplitude)
    {
        time *= Time.timeSinceLevelLoad;

        //calculate frequency
        Vector3 frequency = new Vector3((float)System.Math.Tanh(depth * direction.x),
            (float)System.Math.Tanh(depth * direction.y),
            (float)System.Math.Tanh(depth * direction.z));

        frequency *= (depth * gravity);
        float frequencyValue = Mathf.Sqrt(frequency.x);
        frequencyValue *= time;

        //calculate theta
        float theta = direction.x * position.x + direction.z * position.z;
        theta -= frequencyValue;
        theta -= phase;

        //calculate displacement
        Vector3 result;

        float length = direction.magnitude;

        float dir1 = direction.x / length;
        float dirMultiplier = length * depth;

        dirMultiplier = (float)System.Math.Tanh(dirMultiplier);

        dirMultiplier = amplitude / dirMultiplier;

        //x displacement
        dir1 *= dirMultiplier;

        result.x = Mathf.Sin(theta) * dir1;
        result.x *= -1;

        //z displacement
        float dir2 = direction.z / length;
        dir2 *= dirMultiplier;

        result.z = Mathf.Sin(theta) * dir2;
        result.z *= -1;

        //y displacement
        result.y = Mathf.Cos(theta) * amplitude;

        return result;
    }
}
