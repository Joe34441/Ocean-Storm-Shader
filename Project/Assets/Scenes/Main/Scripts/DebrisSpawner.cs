using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebrisSpawner : MonoBehaviour
{
    [SerializeField] private Transform debrisParent;

    [Header("")]
    [SerializeField] private GameObject spawnBounds;
    [SerializeField] private int maxSpawnAmount;
    [SerializeField] private float maxTravelRange;

    [Header("")]
    [SerializeField] private List<GameObject> debrisVariations = new List<GameObject>();

    private List<GameObject> spawnedObjects = new List<GameObject>();

    private float updateTime = 0.1f;
    private float updateTimer;

    private void Awake()
    {
        Application.targetFrameRate = 144;
    }

    private void Start()
    {
        SpawnObject(Random.Range(0, debrisVariations.Count));
    }

    void Update()
    {
        if (updateTimer < updateTime)
        {
            updateTimer += Time.deltaTime;
            return;
        }
        else
        {
            updateTimer = 0;

            Vector3 spawnPosition = spawnedObjects[spawnedObjects.Count - 1].transform.position;
            float distance = Mathf.Sqrt(spawnPosition.x * spawnPosition.x + spawnPosition.z * spawnPosition.z);

            Vector3 boundsPosition = spawnBounds.transform.position;
            distance -= Mathf.Sqrt(boundsPosition.x * boundsPosition.x + boundsPosition.z * boundsPosition.z);

            if (maxTravelRange / maxSpawnAmount < distance) SpawnObject(Random.Range(0, debrisVariations.Count));

            if (spawnedObjects.Count > 0) CheckDestroyObject(spawnedObjects[0]);
        }
    }

    private void SpawnObject(int newObjectIndex)
    {
        BoxCollider collider = spawnBounds.GetComponent<BoxCollider>();

        Vector3 randomPoint;
        randomPoint.x = Random.Range(-collider.bounds.extents.x, collider.bounds.extents.x);
        randomPoint.y = Random.Range(-collider.bounds.extents.y, collider.bounds.extents.y);
        randomPoint.z = Random.Range(-collider.bounds.extents.z, collider.bounds.extents.z);

        GameObject obj = Instantiate(debrisVariations[newObjectIndex], debrisParent);
        obj.transform.position = randomPoint + collider.transform.position;
        obj.transform.rotation = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));

        obj.AddComponent<ObjectBuoyancy>();

        spawnedObjects.Add(obj);
    }

    private void CheckDestroyObject(GameObject obj)
    {
        float xPos = obj.transform.position.x;
        float zPos = obj.transform.position.z;

        if (Mathf.Sqrt(xPos * xPos + zPos * zPos) > maxTravelRange)
        {
            if (spawnedObjects.Contains(obj)) spawnedObjects.Remove(obj);
            Destroy(obj.GetComponent<ObjectBuoyancy>());
            Destroy(obj, 2.0f);
        }
    }
}
