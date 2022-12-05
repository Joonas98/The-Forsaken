using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Modifioitu videosta https://www.youtube.com/watch?v=YUIohCXt_pc
// Videon kommenteissa korjaus jos vaihtaa vain uuteen input järjestelmään
// Abilityjen tooltipit
public class TooltipUI : MonoBehaviour
{
    [SerializeField] private RectTransform backgroundRectTransform;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private TextMeshProUGUI tooltipText;

    [SerializeField] private RectTransform canvasRectTransform;

    private void Awake()
    {
        // SetText("Penis :D \n" + "Lisää teksti testiä blaa blaa xD xD");
    }

    public void SetText(TextMeshProUGUI newText)
    {
        tooltipText.text = newText.text;
        tooltipText.ForceMeshUpdate();

        Vector2 textSize = tooltipText.GetRenderedValues(false);
        Vector2 paddingSize = new Vector2(tooltipText.margin.x * 2, tooltipText.margin.y * 2);
        // Vector2 paddingSize = new Vector2(8, 8);
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
