using System.Collections;
using UnityEngine;

public class WeatherEffectController : MonoBehaviour
{
    [Header("Rain")]
    [SerializeField] GameObject rainEffect;
    [SerializeField] ParticleSystem[] rainParticles;

    [Header("Wet Ground")]
    [SerializeField] GameObject wetGroundEffect;
    [SerializeField] Renderer[] groundRenderers;
    [SerializeField] Color wetGroundColor = new Color(0.33f, 0.39f, 0.36f, 1f);
    [SerializeField] Terrain[] wetTerrains;
    [SerializeField] Color wetTerrainTint = new Color(0.42f, 0.5f, 0.55f, 1f);
    [SerializeField, Range(0f, 1f)] float wetTerrainSmoothness = 0.85f;
    [SerializeField, Range(0f, 1f)] float wetTerrainMetallic = 0f;

    [Header("Sky And Light")]
    [SerializeField] Light sunLight;
    [SerializeField] Color rainyAmbientColor = new Color(0.18f, 0.2f, 0.22f, 1f);
    [SerializeField] Color rainyFogColor = new Color(0.2f, 0.23f, 0.25f, 1f);
    [SerializeField] float rainySunIntensity = 0.35f;
    [SerializeField] float rainySkyboxExposure = 0.45f;
    [SerializeField] bool enableFogDuringRain = true;
    [SerializeField] float transitionDuration = 1.5f;

    Color initialAmbientColor;
    Color initialFogColor;
    float initialSunIntensity;
    float initialSkyboxExposure = 1f;
    bool initialFog;
    Material runtimeSkybox;
    Color[] initialGroundColors;
    TerrainLayer[][] initialTerrainLayers;
    TerrainLayer[][] runtimeTerrainLayers;
    Coroutine transitionRoutine;

    void Awake()
    {
        initialAmbientColor = RenderSettings.ambientLight;
        initialFogColor = RenderSettings.fogColor;
        initialFog = RenderSettings.fog;

        if (sunLight != null)
            initialSunIntensity = sunLight.intensity;

        if (RenderSettings.skybox != null)
        {
            runtimeSkybox = new Material(RenderSettings.skybox);
            RenderSettings.skybox = runtimeSkybox;

            if (runtimeSkybox.HasProperty("_Exposure"))
                initialSkyboxExposure = runtimeSkybox.GetFloat("_Exposure");
        }

        CacheGroundColors();
        CacheTerrainLayers();
        SetRainActive(false);

        if (wetGroundEffect != null)
            wetGroundEffect.SetActive(false);
    }

    public void StartRain()
    {
        SetRainActive(true);
        ShowWetGround();
        StartWeatherTransition(true);
    }

    public void StopRain(bool keepGroundWet)
    {
        SetRainActive(false);
        StartWeatherTransition(false);

        if (wetGroundEffect != null)
            wetGroundEffect.SetActive(keepGroundWet);
    }

    public void ShowWetGround()
    {
        if (wetGroundEffect != null)
            wetGroundEffect.SetActive(true);

        ApplyGroundColor(wetGroundColor);
        ApplyTerrainWetness();
    }

    public void ResetWeather()
    {
        SetRainActive(false);
        StartWeatherTransition(false);

        if (wetGroundEffect != null)
            wetGroundEffect.SetActive(false);

        RestoreGroundColors();
        RestoreTerrainLayers();
    }

    void StartWeatherTransition(bool rainy)
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(WeatherTransition(rainy));
    }

    IEnumerator WeatherTransition(bool rainy)
    {
        var startAmbient = RenderSettings.ambientLight;
        var startFogColor = RenderSettings.fogColor;
        var startFog = RenderSettings.fog;
        var startSunIntensity = sunLight != null ? sunLight.intensity : 0f;
        var startSkyExposure = GetSkyboxExposure();

        var targetAmbient = rainy ? rainyAmbientColor : initialAmbientColor;
        var targetFogColor = rainy ? rainyFogColor : initialFogColor;
        var targetFog = rainy ? enableFogDuringRain : initialFog;
        var targetSunIntensity = rainy ? rainySunIntensity : initialSunIntensity;
        var targetSkyExposure = rainy ? rainySkyboxExposure : initialSkyboxExposure;

        RenderSettings.fog = targetFog || startFog;

        var time = 0f;
        while (time < transitionDuration)
        {
            time += Time.deltaTime;
            var t = transitionDuration <= 0f ? 1f : Mathf.Clamp01(time / transitionDuration);

            RenderSettings.ambientLight = Color.Lerp(startAmbient, targetAmbient, t);
            RenderSettings.fogColor = Color.Lerp(startFogColor, targetFogColor, t);

            if (sunLight != null)
                sunLight.intensity = Mathf.Lerp(startSunIntensity, targetSunIntensity, t);

            SetSkyboxExposure(Mathf.Lerp(startSkyExposure, targetSkyExposure, t));
            yield return null;
        }

        RenderSettings.ambientLight = targetAmbient;
        RenderSettings.fogColor = targetFogColor;
        RenderSettings.fog = targetFog;

        if (sunLight != null)
            sunLight.intensity = targetSunIntensity;

        SetSkyboxExposure(targetSkyExposure);
        transitionRoutine = null;
    }

    void SetRainActive(bool active)
    {
        if (rainEffect != null)
            rainEffect.SetActive(active);

        if (rainParticles == null)
            return;

        foreach (var particle in rainParticles)
        {
            if (particle == null)
                continue;

            if (active)
                particle.Play();
            else
                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    void CacheGroundColors()
    {
        if (groundRenderers == null)
            return;

        initialGroundColors = new Color[groundRenderers.Length];

        for (var i = 0; i < groundRenderers.Length; i++)
        {
            var rendererItem = groundRenderers[i];
            initialGroundColors[i] = Color.white;

            if (rendererItem != null && rendererItem.material.HasProperty("_Color"))
                initialGroundColors[i] = rendererItem.material.color;
        }
    }

    void ApplyGroundColor(Color color)
    {
        if (groundRenderers == null)
            return;

        foreach (var rendererItem in groundRenderers)
        {
            if (rendererItem != null && rendererItem.material.HasProperty("_Color"))
                rendererItem.material.color = color;
        }
    }

    void RestoreGroundColors()
    {
        if (groundRenderers == null || initialGroundColors == null)
            return;

        for (var i = 0; i < groundRenderers.Length && i < initialGroundColors.Length; i++)
        {
            var rendererItem = groundRenderers[i];

            if (rendererItem != null && rendererItem.material.HasProperty("_Color"))
                rendererItem.material.color = initialGroundColors[i];
        }
    }

    void CacheTerrainLayers()
    {
        if (wetTerrains == null)
            return;

        initialTerrainLayers = new TerrainLayer[wetTerrains.Length][];
        runtimeTerrainLayers = new TerrainLayer[wetTerrains.Length][];

        for (var terrainIndex = 0; terrainIndex < wetTerrains.Length; terrainIndex++)
        {
            var terrain = wetTerrains[terrainIndex];
            if (terrain == null || terrain.terrainData == null)
                continue;

            var sourceLayers = terrain.terrainData.terrainLayers;
            initialTerrainLayers[terrainIndex] = sourceLayers;
            runtimeTerrainLayers[terrainIndex] = new TerrainLayer[sourceLayers.Length];

            for (var layerIndex = 0; layerIndex < sourceLayers.Length; layerIndex++)
            {
                var sourceLayer = sourceLayers[layerIndex];
                runtimeTerrainLayers[terrainIndex][layerIndex] = sourceLayer != null ? Instantiate(sourceLayer) : null;
            }
        }
    }

    void ApplyTerrainWetness()
    {
        if (wetTerrains == null || runtimeTerrainLayers == null)
            return;

        for (var terrainIndex = 0; terrainIndex < wetTerrains.Length; terrainIndex++)
        {
            var terrain = wetTerrains[terrainIndex];
            var layers = terrainIndex < runtimeTerrainLayers.Length ? runtimeTerrainLayers[terrainIndex] : null;

            if (terrain == null || terrain.terrainData == null || layers == null)
                continue;

            foreach (var layer in layers)
            {
                if (layer == null)
                    continue;

                layer.diffuseRemapMin = Vector4.zero;
                layer.diffuseRemapMax = new Vector4(wetTerrainTint.r, wetTerrainTint.g, wetTerrainTint.b, wetTerrainTint.a);
                layer.smoothness = wetTerrainSmoothness;
                layer.metallic = wetTerrainMetallic;
            }

            terrain.terrainData.terrainLayers = layers;
        }
    }

    void RestoreTerrainLayers()
    {
        if (wetTerrains == null || initialTerrainLayers == null)
            return;

        for (var terrainIndex = 0; terrainIndex < wetTerrains.Length; terrainIndex++)
        {
            var terrain = wetTerrains[terrainIndex];
            var layers = terrainIndex < initialTerrainLayers.Length ? initialTerrainLayers[terrainIndex] : null;

            if (terrain != null && terrain.terrainData != null && layers != null)
                terrain.terrainData.terrainLayers = layers;
        }
    }

    float GetSkyboxExposure()
    {
        if (runtimeSkybox != null && runtimeSkybox.HasProperty("_Exposure"))
            return runtimeSkybox.GetFloat("_Exposure");

        return initialSkyboxExposure;
    }

    void SetSkyboxExposure(float exposure)
    {
        if (runtimeSkybox != null && runtimeSkybox.HasProperty("_Exposure"))
            runtimeSkybox.SetFloat("_Exposure", exposure);
    }
}
