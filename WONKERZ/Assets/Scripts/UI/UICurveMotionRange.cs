using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UICurveMotionRange : MonoBehaviour
{
    public Color range_color;
    private Image image;
    private RectTransform rectTransform;

    public void updateTransform( UICurveSelector iUICS, float xAnchor )
    {
        float n = (iUICS.XKeyRightBound-iUICS.XKeyLeftBound)/(iUICS.XRightBound - iUICS.XLeftBound);
        Vector3 new_scale = new Vector3( n, transform.localScale.y, transform.localScale.z );
        transform.localScale = new_scale;
        //float xcenter = iUICS.XRightBound/* + (iUICS.XKeyRightBound-iUICS.XKeyLeftBound)*/;
        float xcenter = Mathf.Sqrt(iUICS.XKeyLeftBound*iUICS.XKeyLeftBound + iUICS.XKeyRightBound*iUICS.XKeyRightBound) / 2;
        //transform.position = center;
        transform.position += new Vector3(xAnchor, 0, 0);
        //transform.position = iAnchor;
        //rectTransform.position = transform.parent.transform.position;
        //rectTransform.offsetMin = new Vector2(iMin, rectTransform.offsetMin.y);
        //rectTransform.offsetMax = new Vector2(iMax, rectTransform.offsetMin.y);

        // todo : continue htis hsit
    }

    void Awake()
    {
        image           = GetComponent<Image>();
        image.color     = range_color;
        rectTransform   = GetComponent<RectTransform>();

    }
}
