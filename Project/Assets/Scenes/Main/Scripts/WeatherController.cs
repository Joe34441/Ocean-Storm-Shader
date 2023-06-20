using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class WeatherController : MonoBehaviour
{
    [SerializeField] private bool enableRain = true;
    [SerializeField] VisualEffect rainVisualEffect;
    private bool rainEnabledSwitch;

    [Header("")]

    [SerializeField] private bool enableFog = true;
    [SerializeField] VisualEffect fogVisualEffect;
    private bool fogEnabledSwitch;
    [SerializeField] private bool killFacingHalfFog = true;
    private bool fogKillFacingHalfSwitch;
    [SerializeField] private bool killTopFog = true;
    private bool fogKillTopSwitch;

    [Header("")]

    [SerializeField] private float windDirectionChangeTime = 6;
    [SerializeField] private float windDirectionChangeLerpTime = 1.0f;
    [SerializeField] private float rainOffsetChangeTime = 3.0f;
    private float windDirectionChangeTimer = 0;
    private float windDirectionChangeLerpTimer = 0;
    private bool windDirectionMoving = false;
    private bool rainOffsetMoving = false;
    private Vector3 oldWindDirection;
    private Vector3 oldSpawnOffset;

    [SerializeField] private Vector3 windDirection1;
    [SerializeField] private Vector3 windDirection2;
    [SerializeField] private Vector3 windDirection3;
    private List<Vector3> windDirections = new List<Vector3>();

    [SerializeField] private Vector3 rainSpawnOffset1;
    [SerializeField] private Vector3 rainSpawnOffset2;
    [SerializeField] private Vector3 rainSpawnOffset3;
    private List<Vector3> rainSpawnOffsets = new List<Vector3>();

    private int currentDirectionIndex = 0;

    private void Start()
    {
        Application.targetFrameRate = 144;
    }

    void Update()
    {
        UpdateVisualEffects();
    }

    private void UpdateVisualEffects()
    {
        if (rainEnabledSwitch != enableRain)
        {
            rainVisualEffect.gameObject.SetActive(enableRain);
            rainEnabledSwitch = enableRain;
        }
        if (enableRain) UpdateRain();

        if (fogEnabledSwitch != enableFog)
        {
            fogVisualEffect.gameObject.SetActive(enableFog);
            fogEnabledSwitch = enableFog;
        }
        if (fogKillFacingHalfSwitch != killFacingHalfFog)
        {
            fogVisualEffect.SetBool("Kill Facing Half", killFacingHalfFog);
            fogKillFacingHalfSwitch = killFacingHalfFog;
        }
        if (fogKillTopSwitch != killTopFog)
        {
            fogVisualEffect.SetBool("Kill Top", killTopFog);
            fogKillTopSwitch = killTopFog;
        }
        if (enableFog)  UpdateFog();
    }

    private void UpdateFog()
    {

    }

    private void UpdateRain()
    {
        if (windDirectionChangeTimer < windDirectionChangeTime) windDirectionChangeTimer += Time.deltaTime;
        else
        {
            windDirectionChangeTimer = 0;
            windDirectionMoving = true;
            rainOffsetMoving = true;

            windDirections.Clear();
            windDirections.Add(windDirection1);
            windDirections.Add(windDirection2);
            windDirections.Add(windDirection3);
            rainSpawnOffsets.Clear();
            rainSpawnOffsets.Add(rainSpawnOffset1);
            rainSpawnOffsets.Add(rainSpawnOffset2);
            rainSpawnOffsets.Add(rainSpawnOffset3);


            oldWindDirection = windDirections[currentDirectionIndex];
            oldSpawnOffset = rainSpawnOffsets[currentDirectionIndex];

            currentDirectionIndex++;
            if (currentDirectionIndex > 2) currentDirectionIndex = 0;

            return;
        }

        if (windDirectionMoving)
        {
            Vector3 newDirection = windDirections[currentDirectionIndex];
            float moveAmount = windDirectionChangeLerpTimer / windDirectionChangeLerpTime;
            newDirection = Vector3.Lerp(oldWindDirection, newDirection, moveAmount);
            rainVisualEffect.SetVector3("Wind Strength", newDirection);

            if (windDirectionChangeLerpTimer < windDirectionChangeLerpTime) windDirectionChangeLerpTimer += Time.deltaTime;
            else windDirectionMoving = false;
        }

        if (rainOffsetMoving)
        {
            Vector3 newDirection = rainSpawnOffsets[currentDirectionIndex];
            float moveAmount = windDirectionChangeLerpTimer / rainOffsetChangeTime;
            newDirection = Vector3.Lerp(oldSpawnOffset, newDirection, moveAmount);
            rainVisualEffect.SetVector3("Spawn Bounds Offset", newDirection);

            if (windDirectionChangeLerpTimer < rainOffsetChangeTime) windDirectionChangeLerpTimer += Time.deltaTime;
            else
            {
                windDirectionChangeLerpTimer = 0;
                rainOffsetMoving = false;
            }
        }
    }
}
