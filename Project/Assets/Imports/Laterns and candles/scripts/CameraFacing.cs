using UnityEngine;

[ExecuteInEditMode]
public class CameraFacing : MonoBehaviour
{
	public Camera cameraToLookAt;

	private float updateTime = 0.1f;
	private float updateTimer = 0;

    void Update() 
	{
		if (updateTimer < updateTime) updateTimer += Time.deltaTime;
		else
        {
			updateTimer = 0;
			cameraToLookAt = Camera.current;

			if (cameraToLookAt == null) return;

			Vector3 v = cameraToLookAt.transform.position - transform.position;
			v.x = v.z = 0.0f;
			transform.LookAt(cameraToLookAt.transform.position - v);
		}
	}
}