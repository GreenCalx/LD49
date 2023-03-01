using UnityEngine;

public class TrailRenderer : MonoBehaviour
{
    private Camera cam;
    public MeshRenderer MR; //desert for now

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        MR.sharedMaterial.SetVector("_TracesCenter", cam.transform.position);
        MR.sharedMaterial.SetFloat("_TracesSize", cam.orthographicSize);
    }
}
