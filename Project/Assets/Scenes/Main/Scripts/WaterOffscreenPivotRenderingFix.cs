using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WaterOffscreenPivotRenderingFix : MonoBehaviour
{
    private float updateTime = 5.0f;
    private float updateTimer = 5.0f;

    private void Update()
    {
        if (updateTimer < updateTime) updateTimer += Time.deltaTime;
        else
        {
            updateTimer = 0;

            Renderer renderer = GetComponent<Renderer>();
            Bounds bounds = new Bounds(renderer.bounds.center, renderer.bounds.size * 2.5f);
            GetComponent<Renderer>().bounds = bounds;
        }
    }
}
