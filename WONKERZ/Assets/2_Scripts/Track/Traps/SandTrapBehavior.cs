using UnityEngine;

namespace Wonkerz {
    public class SandTrapBehavior : MonoBehaviour
    {

        public ParticleSystem sandParticles;
        public ParticleSystem smokeParts;
        bool focused;
        GameObject focus;

        public Animator trapAnim;
        public Animator fourmillionAnim;
        public Transform fourmillionRoot;
        public Material mat;
        public Material matIdle;
        public MeshRenderer renderer;
        public MeshFilter meshFilter;
        [HideInInspector]
        public Mesh mesh;
        private Vector3[] vertices;
        //public SchSandTrapRigidBodyBehaviour rb;
        public float maxForce;

        public float shootSpeedDamp = 0.5f;

        private int vMid0 = 129;
        private int vMid1 = 163;

        public float maxDepth = 10;

        // 0-1 for maxdepth; is animated
        public float depthPercent = 0;

        // below 1 slower, above faster
        public float roatSpeedTowardsPlayer = 1f;

        // Start is called before the first frame update
        void Start()
        {
            mesh = meshFilter.mesh;
            vertices = mesh.vertices;

            sandParticles.Stop();
            smokeParts.gameObject.SetActive(true);
            smokeParts.Stop();

            mat.SetFloat("_AngleAnimationStart", 0);
            mat.SetFloat("_AngleAnimationStop", 0);
            //rb.rb.MaxForce = 0f;
            renderer.sharedMaterial = matIdle;
        }

        void OnTriggerEnter(Collider c)
        {
            if (!Utils.colliderIsPlayer(c))
            return;

            if (focused) return;

            sandParticles.Play();
            smokeParts.Play();

            focus = c.gameObject;
            focused = true;

            mat.SetFloat("_AngleAnimationStop", Mathf.PI*2);
            renderer.sharedMaterial = mat;
        }

        void OnTriggerExit(Collider c)
        {
            if (!Utils.colliderIsPlayer(c))
            return;

            sandParticles.Stop();
            smokeParts.Stop();

            focus = null;
            focused = false;



            trapAnim.Play("Fourmillion 0");
            mat.SetFloat("_AngleAnimationStop", Mathf.PI*2);
            renderer.sharedMaterial = matIdle;
        }

        // Update is called once per frame
        void LateUpdate()
        {
            trapAnim.SetBool("Focus", focused);
            fourmillionAnim.SetBool("Focus", focused);

            var depth = (-depthPercent * maxDepth) / transform.lossyScale.z;
            if (vertices[vMid0].y != depth)
            {
                vertices[vMid0].y = -1+depth;
                vertices[vMid1].y = depth;
                //mesh.vertices = vertices;

                var meshCollider = GetComponent<MeshCollider>();
                //meshCollider.sharedMesh = mesh;

                var s = smokeParts.shape;
                s.position = new Vector3(0, depth, 0);
            }

            if (focused)
            {
                //rb.rb.MaxForce = maxForce;

                var focusPos = focus.transform.position;
                focusPos.y = fourmillionRoot.gameObject.transform.position.y;

                var lookPos = Access.Player().transform.position - fourmillionRoot.position;
                Quaternion lookRot = Quaternion.LookRotation(lookPos);
                lookRot.eulerAngles =new Vector3(fourmillionRoot.rotation.eulerAngles.x, lookRot.eulerAngles.y, fourmillionRoot.rotation.eulerAngles.z);
                fourmillionRoot.rotation = Quaternion.Slerp(fourmillionRoot.rotation, lookRot, Time.deltaTime * roatSpeedTowardsPlayer);
                sandParticles.transform.rotation = fourmillionRoot.rotation;

                var dir = focus.transform.position - transform.position;

                var angle = Mathf.Abs(sandParticles.shape.rotation.x) * Mathf.Deg2Rad;
                var yDiff = -dir.y;
                dir.y = 0;

                var xDiff = dir.magnitude;

                var g = Physics.gravity.y;

                var main = sandParticles.main;
                //var v = Mathf.Sqrt(Mathf.Abs(g * x * x / (2 * (y - x*Mathf.Tan(angle))))) / Mathf.Cos(angle);
                var v = (xDiff / (Mathf.Sqrt(Mathf.Abs((2 * (yDiff - xDiff*Mathf.Tan(angle)))/g)) * Mathf.Cos(angle)));
                main.startSpeed = v * shootSpeedDamp;

            }
        }
    }
}
