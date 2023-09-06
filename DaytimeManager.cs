using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DaytimeManager : MonoBehaviour
{
    [SerializeField] private float timeMultiplier, startHour;
    [SerializeField] private float sunriseHour, sunsetHour;
    [SerializeField] private float maxSunLightIntensity, maxMoonLightIntensity;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Light sunLight, moonLight;
    [SerializeField] private Color dayAmbientLight, nightAmbientLight;
    [SerializeField] private AnimationCurve lightChangeCurve;
    [SerializeField] Material skyboxDay, skyboxNight;

    private DateTime currentTime;
    private TimeSpan sunriseTime;
    private TimeSpan sunsetTime;

    [Header("Fog")]
    [SerializeField] private float dayFogDensity;
    [SerializeField] private float nightFogDensity;
    [SerializeField] private AnimationCurve fogDensityCurve;

    void Start()
    {
        currentTime = DateTime.Now.Date + TimeSpan.FromHours(startHour);

        sunriseTime = TimeSpan.FromHours(sunriseHour);
        sunsetTime = TimeSpan.FromHours(sunsetHour);
    }

    void Update()
    {
        UpdateTimeOfDay();
        RotateSunAndMoon();
        UpdateLightSettings();
        UpdateFog();
        UpdateSkybox();
    }

    private void UpdateTimeOfDay()
    {
        currentTime = currentTime.AddSeconds(Time.deltaTime * timeMultiplier);

        if (timeText != null)
        {
            timeText.text = currentTime.ToString("HH:mm");
        }
    }

    private void RotateSunAndMoon()
    {
        float sunLightRotation;

        if (currentTime.TimeOfDay > sunriseTime && currentTime.TimeOfDay < sunsetTime)
        {
            TimeSpan sunriseToSunsetDuration = CalculateTimeDifference(sunriseTime, sunsetTime);
            TimeSpan timeSinceSunrise = CalculateTimeDifference(sunriseTime, currentTime.TimeOfDay);

            double percentage = timeSinceSunrise.TotalMinutes / sunriseToSunsetDuration.TotalMinutes;

            sunLightRotation = Mathf.Lerp(0, 180, (float)percentage);
        }
        else
        {
            TimeSpan sunsetToSunriseDuration = CalculateTimeDifference(sunsetTime, sunriseTime);
            TimeSpan timeSinceSunset = CalculateTimeDifference(sunsetTime, currentTime.TimeOfDay);

            double percentage = timeSinceSunset.TotalMinutes / sunsetToSunriseDuration.TotalMinutes;

            sunLightRotation = Mathf.Lerp(180, 360, (float)percentage);
        }

        sunLight.transform.rotation = Quaternion.AngleAxis(sunLightRotation, Vector3.right);
        moonLight.transform.rotation = Quaternion.AngleAxis(sunLightRotation + 180, Vector3.right);
    }

    private void UpdateLightSettings()
    {
        float dotProduct = Vector3.Dot(sunLight.transform.forward, Vector3.down);
        sunLight.intensity = Mathf.Lerp(0, maxSunLightIntensity, lightChangeCurve.Evaluate(dotProduct));
        moonLight.intensity = Mathf.Lerp(maxMoonLightIntensity, 0, lightChangeCurve.Evaluate(dotProduct));
        RenderSettings.ambientLight = Color.Lerp(nightAmbientLight, dayAmbientLight, lightChangeCurve.Evaluate(dotProduct));
    }

    private void UpdateFog()
    {
        // Calculate a normalized time value between 0 (night) and 1 (day)
        float normalizedTime = Mathf.InverseLerp(sunriseHour, sunsetHour, (float)currentTime.TimeOfDay.TotalHours);

        // Use the curve to interpolate between day and night fog densities
        float targetFogDensity = Mathf.Lerp(nightFogDensity, dayFogDensity, fogDensityCurve.Evaluate(normalizedTime));

        RenderSettings.fogColor = RenderSettings.ambientLight;
        RenderSettings.fogDensity = targetFogDensity;
    }

    private void UpdateSkybox()
    {
        float dotProduct = Vector3.Dot(sunLight.transform.forward, Vector3.down);
        _ = Color.Lerp(nightAmbientLight, dayAmbientLight, lightChangeCurve.Evaluate(dotProduct));

        Material blendedSkybox = new(RenderSettings.skybox);
        blendedSkybox.Lerp(skyboxNight, skyboxDay, lightChangeCurve.Evaluate(dotProduct));

        // Optionally, set the color of the blended skybox based on the current ambient color.
        // blendedSkybox.SetColor("_Color", currentAmbientColor);

        RenderSettings.skybox = blendedSkybox;
    }

    private TimeSpan CalculateTimeDifference(TimeSpan fromTime, TimeSpan toTime)
    {
        TimeSpan difference = toTime - fromTime;

        if (difference.TotalSeconds < 0)
        {
            difference += TimeSpan.FromHours(24);
        }

        return difference;
    }
}
