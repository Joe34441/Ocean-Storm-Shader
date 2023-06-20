using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[ExecuteInEditMode]
public class Lightning : MonoBehaviour
{
    [SerializeField] VisualEffect lightningVisualEffect;
    [SerializeField] Light lightningLight;
    bool activated = false;
    private float lightWaitTime = 0.2f;
    private float lightTimer;

    private float waitTime = 8.0f;
    private float waitTimer;

    private void Awake()
    {
        if (lightningVisualEffect == null) lightningVisualEffect = GetComponent<VisualEffect>();
    }

    // Update is called once per frame
    void Update()
    {
        if (waitTimer < waitTime) waitTimer += Time.deltaTime;
        else
        {
            activated = true;
            waitTimer = 0;

            float randomX = Random.Range(-500, 500);
            float randomZ = Random.Range(-500, 500);
            if (Mathf.Abs(randomX) < 200) randomX *= 2;
            if (Mathf.Abs(randomZ) < 200) randomZ *= 2;
            Vector3 newPos = new Vector3(randomX, 300, randomZ);

            Vector3 spawnPos = newPos;
            lightningVisualEffect.SetVector3("Start Pos", spawnPos);

            float newX = randomX + Random.Range(-75, 75);
            float newY = Random.Range(175, 225);
            float newZ = randomZ + Random.Range(-75, 75);
            newPos = new Vector3(newX, newY, newZ);
            lightningVisualEffect.SetVector3("Pos 2", newPos);

            newX += Random.Range(-50, 50);
            newY = Random.Range(75, 125);
            newZ += Random.Range(-50, 50);
            newPos = new Vector3(newX, newY, newZ);
            lightningVisualEffect.SetVector3("Pos 3", newPos);

            newX += Random.Range(-25, 25);
            newY = -25;
            newZ += Random.Range(-25, 25);
            newPos = new Vector3(newX, newY, newZ);
            lightningVisualEffect.SetVector3("End Pos", newPos);

            lightningVisualEffect.Play();

            lightningLight.transform.position = spawnPos;
            Invoke("DisableLight", 2);
        }

        if (activated)
        {
            if (lightTimer < lightWaitTime) lightTimer += Time.deltaTime;
            else
            {
                lightTimer = 0;
                lightningLight.intensity = Random.Range(20000, 100000);
                lightningLight.gameObject.SetActive(true);
            }
        }
        else
        {
            lightTimer = 0;
            lightningLight.gameObject.SetActive(false);
        }
    }

    private void DisableLight()
    {
        activated = false;
    }
}
