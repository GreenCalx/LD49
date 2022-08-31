using UnityEngine;

public class PostProcess : MonoBehaviour
{
    public Material mat;
    private Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        cam.depthTextureMode = cam.depthTextureMode | DepthTextureMode.DepthNormals;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Matrix4x4 projectionMatrix = cam.projectionMatrix;
        projectionMatrix = GL.GetGPUProjectionMatrix(projectionMatrix, false);
        projectionMatrix.SetColumn(1, projectionMatrix.GetColumn(1)*-1);

        Matrix4x4 viewMatrix = cam.worldToCameraMatrix;

        mat.SetMatrix("_UNITY_MATRIX_I_V", viewMatrix.inverse);
        mat.SetMatrix("_UNITY_MATRIX_I_P", projectionMatrix.inverse);
        mat.SetMatrix("_UNITY_MATRIX_I_VP", (projectionMatrix * viewMatrix).inverse);
        Graphics.Blit(source, destination, mat);
    }
}
