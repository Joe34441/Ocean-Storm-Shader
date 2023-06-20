using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class TessellationMousePosition : MonoBehaviour
{
    [Tooltip("Enable to interact with glass. \n(While playing, toggle with CTRL + T) \n \n!!! DISABLE AFTER USE !!!")]
    [SerializeField] public bool active = false;
    [SerializeField] public float activeTime = 3.0f;
    private bool activated = false;
    private float activeTimer = 0.0f;

    private string tagName = "Glass";

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnScene;
    }

    void OnScene(SceneView scene)
    {
        if (this == null) return; //"object is null" OR "DESTROYED BUT NOT NULL"

        Event e = Event.current;

        if (Application.isPlaying)
        {
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.T && e.control) active = !active;
        }

        if (active)
        { 
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                Vector3 mousePos = e.mousePosition;
                float ppp = EditorGUIUtility.pixelsPerPoint;
                mousePos.y = scene.camera.pixelHeight - mousePos.y * ppp;
                mousePos.x *= ppp;

                Ray ray = scene.camera.ScreenPointToRay(mousePos);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform == null) return;

                    foreach (TessellationMousePosition script in FindObjectsOfType<TessellationMousePosition>()) script.active = false;

                    if (hit.transform.CompareTag(tagName))
                    {
                        if (hit.transform == this.transform)
                        {
                            active = true;
                            Vector4 position = new Vector4(hit.point.x, hit.point.y, hit.point.z, 1);
                            GetComponent<Renderer>().material.SetVector("_SourcePosition", position);
                            GetComponent<Renderer>().material.SetInteger("_DrawCracks", 1);
                            activated = true;
                            activeTimer = 0.0f;
                        }
                        else hit.transform.GetComponent<TessellationMousePosition>().active = false;
                    }
                }
                else
                {
                    active = false;
                }
                e.Use();
            }
        }
    }

    private void Start()
    {
        ResetTessellationMaterial();
    }

    void Update()
    {
        if (activated)
        {
            activeTimer += Time.deltaTime;
            if (activeTimer > activeTime)
            {
                activated = false;
                activeTimer = 0.0f;

                ResetTessellationMaterial();
            }
        }
    }

    private void ResetTessellationMaterial()
    {
        Material newMaterial = GetComponent<Renderer>().sharedMaterial;
        newMaterial.SetVector("_SourcePosition", Vector4.zero);
        newMaterial.SetInteger("_DrawCracks", 0);
        GetComponent<Renderer>().material = newMaterial;
    }
}
