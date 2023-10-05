using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitmarkAndCrosshair : MonoBehaviour
{
	// 5.10.2023 new combined script to handle hitmarkers and crosshair
	public static HitmarkAndCrosshair instance;

	[Header("Settings")]
	public bool useHitmarker;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Destroy(gameObject);
		}
	}

	public void Hitmarker(Vector3 hitPosition, bool isHeadshot)
	{
		if (!useHitmarker) return;
		// Object pooled hitmarkers
		GameObject hitmark = ObjectPool.SharedInstance.GetPooledObject();
		if (hitmark != null)
		{
			Image hitImage = hitmark.GetComponent<Image>();
			Vector3 screenPos = Camera.main.WorldToScreenPoint(hitPosition);
			hitImage.rectTransform.position = screenPos;
			hitImage.color = new Color(1, 1, 1, 1);
			hitmark.SetActive(true);
			StartCoroutine(FadeImage(hitImage, isHeadshot));
			StartCoroutine(DisableDelay(hitmark));
		}
	}

	IEnumerator FadeImage(Image image, bool headshot)
	{
		if (!headshot)
		{
			for (float i = 1; i >= -1; i -= Time.deltaTime * 5f)
			{
				image.color = new Color(1, 1, 1, i);
				yield return null;
			}
		}
		else
		{
			for (float i = 1; i >= -1; i -= Time.deltaTime * 5f)
			{
				image.color = new Color(2, 0.1f, 0.1f, i);
				yield return null;
			}
		}
	}

	IEnumerator DisableDelay(GameObject go)
	{
		yield return new WaitForSeconds(1f);
		go.SetActive(false);
	}

}
