using UnityEngine;
using Schnibble;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class WaterRippleCamera : MonoBehaviour
{
    private Camera cam;
    public MeshRenderer waterPlane;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (!waterPlane)
        {
            this.LogWarn("No water plane detected for WaterRippleCamera.");
        }
    }

    private void Update()
    {
        if (!waterPlane)
            return;
        waterPlane.sharedMaterial.SetVector("_CamPosition", transform.position);
        waterPlane.sharedMaterial.SetFloat("_OrthographicCamSize", cam.orthographicSize);
    }
}
