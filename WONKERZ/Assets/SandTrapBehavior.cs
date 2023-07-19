using UnityEngine;

public class SandTrapBehavior : MonoBehaviour
{

    public ParticleSystem sandParticles;
    public ParticleSystem smokeParts;
    public GameObject sandworm;
    bool focused;
    GameObject focus;

    public Animator anim;
    public Transform sandwormRoot;
    public Material mat;
    public Material matIdle;
    public MeshRenderer renderer;
    public MeshFilter meshFilter;
    public Mesh mesh;
    private Vector3[] vertices;
    public SchSandTrapRigidBody rb;
    public float maxForce;

    private int vMid0 = 129;
    private int vMid1 = 163;

    public float maxDepth = 10;

    // 0-1 for maxdepth; is animated
    public float depthPercent = 0;

    // Start is called before the first frame update
    void Start()
    {
        mesh = meshFilter.mesh;
        vertices = mesh.vertices;

        sandParticles.Stop();
        smokeParts.Stop();

        mat.SetFloat("_AngleAnimationStart", 0);
        mat.SetFloat("_AngleAnimationStop", 0);
        rb.MaxForce = 0f;
        renderer.sharedMaterial = matIdle;
    }

    void OnTriggerEnter(Collider c)
    {
        if (focused) return;

        focus = c.gameObject;
        focused = true;
        sandParticles.Play();
        smokeParts.Play();
        mat.SetFloat("_AngleAnimationStop", Mathf.PI*2);
        renderer.sharedMaterial = mat;
    }

    void OnTriggerExit(Collider c)
    {
        focus = null;
        focused = false;

        sandParticles.Stop();
        smokeParts.Stop();

        anim.Play("Fourmillion 0");
        mat.SetFloat("_AngleAnimationStop", Mathf.PI*2);
        renderer.sharedMaterial = matIdle;
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetBool("Focus", focused);

        var depth = (-depthPercent * maxDepth) / transform.lossyScale.z;
        if (vertices[vMid0].y != depth)
        {
            vertices[vMid0].y = -1+depth;
            vertices[vMid1].y = depth;
            mesh.vertices = vertices;

            var meshCollider = GetComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;

            var s = smokeParts.shape;
            s.position = new Vector3(0, depth, 0);
        }

        if (focused)
        {
            rb.MaxForce = maxForce;

            var focusPos = focus.transform.position;
            focusPos.y = sandwormRoot.gameObject.transform.position.y;
            sandwormRoot.gameObject.transform.LookAt(focusPos);

            var dir = focus.transform.position - transform.position;

            var angle = Mathf.Abs(sandParticles.shape.rotation.x) * Mathf.Deg2Rad;
            var yDiff = -dir.y;
            dir.y = 0;

            var xDiff = dir.magnitude;

            var g = Physics.gravity.y;

            var main = sandParticles.main;
            //var v = Mathf.Sqrt(Mathf.Abs(g * x * x / (2 * (y - x*Mathf.Tan(angle))))) / Mathf.Cos(angle);
            var v = (xDiff / (Mathf.Sqrt(Mathf.Abs((2 * (yDiff - xDiff*Mathf.Tan(angle)))/g)) * Mathf.Cos(angle)));
            main.startSpeed = v;
        }
    }
}
