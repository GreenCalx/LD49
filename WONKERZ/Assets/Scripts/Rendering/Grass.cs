using UnityEngine;

[ExecuteInEditMode]
public class Grass : MonoBehaviour
{
    public Mesh Ground;
    public GameObject Island;

    public ComputeShader GrassCS;
    public Material GrassMaterial;
    public GameObject InteractWithGO;

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct SourceVertex
    {
        public Vector3 position;
    };

    [System.Serializable]
    public struct GrassSettings
    {
        public int _MaxBladeSegments;
        public float _BladeCurvature;
        public float _MaxBendAngle;
        public float _BladeHeight;
        public float _BladeHeightVariance;
        public float _BladeWidth;
        public float _BladeWidthVariance;
        public Vector3 centerPosition;
        public float _BladeDensity;
    };
    public GrassSettings settings;

    private bool Initialized = false;

    private ComputeBuffer vertBuff;
    private ComputeBuffer triBuff;
    private ComputeBuffer drawBuffer;
    private ComputeBuffer argsBuffer;

    private int KernelId;
    private int DispatchSize;
    private Bounds LocalBounds;

    private const int SOURCE_VERTEX_STRIDE = sizeof(float) * 3;
    private const int SOURCE_TRI_STRIDE = sizeof(int);
    private const int DRAW_STRIDE = sizeof(float) * (3 + (3 + 1) * 3);
    private const int INDIRECT_ARGS_STRIDE = sizeof(int) * 4;

    private int[] argsBufferReset = new int[] { 0, 1, 0, 0 };

    void Start()
    {
        if (InteractWithGO==null)
        {
            InteractWithGO = Access.Player().gameObject;
        }
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        if (Initialized) OnDisable();
        Initialized = true;

        if (Island)
        {
            Ground = Island.GetComponent<MeshFilter>().sharedMesh;
        }
        Vector3[] positions = Ground.vertices;
        int[] tris = Ground.triangles;

        SourceVertex[] vertices = new SourceVertex[positions.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            Matrix4x4 t = (Island != null ? Island.transform.localToWorldMatrix : Matrix4x4.identity);
            vertices[i] = new SourceVertex()
            {
                position = Island.transform.position + (Island.transform.rotation * Vector3.Scale(positions[i], Island.transform.lossyScale)),
            };
        }

        int numTriangles = tris.Length / 3;
        int maxBladeSegments = Mathf.Max(1, settings._MaxBladeSegments);
        int maxBladeTriangles = (maxBladeSegments - 1) * 2 + 1;

        vertBuff = new ComputeBuffer(vertices.Length, SOURCE_VERTEX_STRIDE, ComputeBufferType.Structured, ComputeBufferMode.Immutable);
        vertBuff.SetData(vertices);

        triBuff = new ComputeBuffer(tris.Length, SOURCE_TRI_STRIDE, ComputeBufferType.Structured, ComputeBufferMode.Immutable);
        triBuff.SetData(tris);

        drawBuffer = new ComputeBuffer(100 * numTriangles * maxBladeTriangles * 2, DRAW_STRIDE, ComputeBufferType.Append);
        drawBuffer.SetCounterValue(0);

        argsBuffer = new ComputeBuffer(1, INDIRECT_ARGS_STRIDE, ComputeBufferType.IndirectArguments);

        KernelId = GrassCS.FindKernel("CSMain");

        GrassCS.SetBuffer(KernelId, "_SourceVertices", vertBuff);
        GrassCS.SetBuffer(KernelId, "_SourceTriangles", triBuff);
        GrassCS.SetBuffer(KernelId, "_DrawTriangles", drawBuffer);
        GrassCS.SetBuffer(KernelId, "_IndirectArgsBuffer", argsBuffer);

        GrassCS.SetFloat("_MaxBendAngle", settings._MaxBendAngle);
        GrassCS.SetFloat("_BladeHeight", settings._BladeHeight);
        GrassCS.SetFloat("_BladeHeightVariance", settings._BladeHeightVariance);
        GrassCS.SetFloat("_BladeWidth", settings._BladeWidth);
        GrassCS.SetFloat("_BladeWidthVariance", settings._BladeWidthVariance);
        GrassCS.SetInt("_MaxBladeSegments", settings._MaxBladeSegments);
        GrassCS.SetFloat("_BladeCurvature", settings._BladeCurvature);
        GrassCS.SetFloat("_BladeDensity", settings._BladeDensity);


        GrassCS.SetInt("_NumSourceTriangles", numTriangles);

        GrassMaterial.SetBuffer("_DrawTriangles", drawBuffer);

        GrassCS.GetKernelThreadGroupSizes(KernelId, out uint ThreadGroupSize, out _, out _);
        DispatchSize = Mathf.CeilToInt((float)numTriangles / ThreadGroupSize);

        LocalBounds = Ground.bounds;
        LocalBounds.Expand(Mathf.Max(settings._BladeHeight + settings._BladeHeightVariance, settings._BladeWidth + settings._BladeWidthVariance));
    }

    void OnDisable()
    {
        if (Initialized)
        {
            vertBuff.Release();
            triBuff.Release();
            drawBuffer.Release();
            argsBuffer.Release();
        }
        Initialized = false;
    }

    Bounds TransformBoundsIsland(Bounds B)
    {
        var center = Island.transform.rotation * Vector3.Scale(B.center, Island.transform.lossyScale);

        var extents = B.extents;
        var axisX = Island.transform.rotation * Vector3.Scale(new Vector3(extents.x, 0, 0), Island.transform.lossyScale);
        var axisY = Island.transform.rotation * Vector3.Scale(new Vector3(0, extents.y, 0), Island.transform.lossyScale);
        var axisZ = Island.transform.rotation * Vector3.Scale(new Vector3(0, 0, extents.z), Island.transform.lossyScale);

        extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
        extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
        extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

        return new Bounds { center = center, extents = extents };

    }

    Bounds TransformBounds(Bounds B)
    {
        var center = transform.TransformPoint(B.center);

        var extents = B.extents;
        var axisX = transform.TransformVector(extents.x, 0, 0);
        var axisY = transform.TransformVector(0, extents.y, 0);
        var axisZ = transform.TransformVector(0, 0, extents.z);

        extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
        extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
        extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

        return new Bounds { center = center, extents = extents };
    }

    // Update is called once per frame
    void LateUpdate()
    {

        if (!Application.isPlaying)
        {
            OnDisable();
            OnEnable();
        }

        drawBuffer.SetCounterValue(0);
        argsBuffer.SetData(argsBufferReset);

        Bounds B = TransformBoundsIsland(LocalBounds);

        GrassCS.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
        GrassCS.Dispatch(KernelId, DispatchSize, 1, 1);
        if (InteractWithGO!=null)
            settings.centerPosition = InteractWithGO.transform.position;
        float[] CP = { settings.centerPosition.x, settings.centerPosition.y, settings.centerPosition.z };
        GrassCS.SetFloats("_CenterPositionWS", CP);

        Graphics.DrawProceduralIndirect(GrassMaterial, B, MeshTopology.Triangles, argsBuffer, 0, null, null, UnityEngine.Rendering.ShadowCastingMode.Off, true, gameObject.layer);
    }
}
