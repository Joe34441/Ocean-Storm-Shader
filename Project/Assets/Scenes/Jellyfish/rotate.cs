using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class rotate : MonoBehaviour
{
    [SerializeField] float rotation = 10.0f;

    void Update()
    {
        Vector3 newRot = transform.eulerAngles;
        newRot.z += rotation * Time.deltaTime;

        if (newRot.z > 360) newRot.z -= 360;

        transform.rotation = Quaternion.Euler(newRot);
    }
}
