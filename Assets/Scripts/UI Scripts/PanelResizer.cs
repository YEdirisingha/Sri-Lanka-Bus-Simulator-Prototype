using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelResizer : MonoBehaviour
{
    [Range(0f, 1f)] public float widthPercentage = 0.5f;  // 50% of screen width
    [Range(0f, 1f)] public float heightPercentage = 0.5f; // 50% of screen height

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        ResizePanel();
    }

    void ResizePanel()
    {
        float newWidth = Screen.width * widthPercentage;
        float newHeight = Screen.height * heightPercentage;
        rectTransform.sizeDelta = new Vector2(newWidth, newHeight);
    }

    void Update()
    {
        // Optional: update dynamically if screen size changes (e.g., mobile rotation)
        if (Screen.width != rectTransform.rect.width || Screen.height != rectTransform.rect.height)
        {
            ResizePanel();
        }
    }
}
