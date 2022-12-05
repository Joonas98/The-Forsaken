using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{

    [SerializeField] private RectTransform crosshair;

    public float lerpSpeed;
    public float currentSize;
    public float amountMultiplier;
    public float crosshairAdjustment;

    public void AdjustCrosshair(float amount)
    {
        currentSize = Mathf.Lerp(currentSize, amount * amountMultiplier, Time.deltaTime * lerpSpeed);
    }

    private void Update()
    {
        crosshair.sizeDelta = new Vector2(currentSize + crosshairAdjustment, currentSize + crosshairAdjustment);
    }

}
