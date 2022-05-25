using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UICurveMotionRange : MonoBehaviour
{
    private float min, max;
    public Color range_color;
    private Image image;
    private RectTransform rectTransform;
    public void setMotionRange( float iMin, float iMax)
    {
        min = iMin;
        max = iMax;
        rectTransform.offsetMin = new Vector2(iMin, rectTransform.offsetMin.y);
        rectTransform.offsetMax = new Vector2(iMax, rectTransform.offsetMin.y);
        // todo : continue htis hsit
    }

    void Start()
    {
        image           = GetComponent<Image>();
        image.color = range_color;
        rectTransform   = GetComponent<RectTransform>();

    }
}
