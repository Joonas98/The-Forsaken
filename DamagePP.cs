using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class DamagePP : MonoBehaviour
{

    public PostProcessVolume volume;

    public float maxBloom;
    public float maxVignette;
    public float maxChromaticAberration;
    public float maxGrain;

    private Bloom bloom;
    private Vignette vignette;
    private ChromaticAberration chromaticAberration;
    private Grain grain;

    void Start()
    {
        volume.profile.TryGetSettings(out bloom);
        volume.profile.TryGetSettings(out vignette);
        volume.profile.TryGetSettings(out chromaticAberration);
        volume.profile.TryGetSettings(out grain);
    }

    void Update()
    {
       // if (Input.GetKeyDown(KeyCode.P))
       // {
       //     UpdateDamagePP(maxBloom, maxVignette, maxChromaticAberration, maxGrain);
       // }
    }

    public void UpdateDamagePP(float bloomValue, float vignetteValue, float chromaticAberrationValue, float grainValue)
    {
        bloom.intensity.value = bloomValue;
        vignette.intensity.value = vignetteValue;
        chromaticAberration.intensity.value = chromaticAberrationValue;
        grain.intensity.value = grainValue;
    }

}
