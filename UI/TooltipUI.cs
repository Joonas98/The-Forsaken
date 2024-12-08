using TMPro;
using UnityEngine;

// Original base from https://www.youtube.com/watch?v=YUIohCXt_pc
// Ability tooltips
public class TooltipUI : MonoBehaviour
{
	[SerializeField] private RectTransform backgroundRectTransform;
	[SerializeField] private RectTransform rectTransform;
	[SerializeField] private TextMeshProUGUI tooltipText;

	[SerializeField] private RectTransform canvasRectTransform;

	public void SetText(int index)
	{
		// tooltipText.text = AbilityMaster.instance.GetAbilityDescription(index);
		tooltipText.ForceMeshUpdate();
		Vector2 textSize = tooltipText.GetRenderedValues(false);
		Vector2 paddingSize = new Vector2(tooltipText.margin.x * 2, tooltipText.margin.y * 2);
		backgroundRectTransform.sizeDelta = textSize + paddingSize;
	}

	private void Update()
	{
		Vector2 anchoredPosition = Input.mousePosition / canvasRectTransform.localScale.x;

		if (anchoredPosition.x + backgroundRectTransform.rect.width > canvasRectTransform.rect.width)
		{
			anchoredPosition.x = canvasRectTransform.rect.width - backgroundRectTransform.rect.width;
		}

		if (anchoredPosition.y + backgroundRectTransform.rect.height > canvasRectTransform.rect.height)
		{
			anchoredPosition.y = canvasRectTransform.rect.height - backgroundRectTransform.rect.height;
		}

		Rect screenRect = new Rect(0, 0, Screen.currentResolution.width, Screen.currentResolution.height);
		if (anchoredPosition.x < screenRect.x)
		{
			anchoredPosition.x = screenRect.x;
		}

		if (anchoredPosition.y < screenRect.y)
		{
			anchoredPosition.y = screenRect.y;
		}

		rectTransform.anchoredPosition = anchoredPosition;
	}

}
