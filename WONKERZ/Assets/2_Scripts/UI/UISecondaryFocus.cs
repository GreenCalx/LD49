using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UISecondaryFocus : MonoBehaviour
{
    [Header("Mandatory")]
    public RectTransform parentCanvasRect;
    public RectTransform selfRect;
    [Header("Internals")]
    public Camera cam;
    public Transform trackedObject;
    private Image img;

    public void trackObjectPosition(Transform iWorldTransform)
    {
        trackedObject = iWorldTransform;
        cam = Access.CameraManager().active_camera.cam;
        img = GetComponent<Image>();
    }

    private void projectPosition()
    {
        Vector3 projectionOnCam = cam.WorldToViewportPoint(trackedObject.position);
        float xPos = projectionOnCam.x * parentCanvasRect.sizeDelta.x - parentCanvasRect.sizeDelta.x*0.5f;
        float yPos = projectionOnCam.y * parentCanvasRect.sizeDelta.y - parentCanvasRect.sizeDelta.y*0.5f;

        Vector2 screenPos = new Vector2(xPos, yPos);
        selfRect.anchoredPosition = screenPos;
    }

    public void setColor(Color iColor)
    {
        img.color = iColor;
    }

    public void updateFillAmount(float iFill)
    {
        float v = Mathf.Clamp(iFill, 0f, 1f);
        img.fillAmount = 1f - v;
    }

    // Update is called once per frame
    void Update()
    {
        if (!!trackedObject)
            projectPosition();
    }
}
