using UnityEngine;
using Schnibble;

namespace Wonkerz {
    public class Resetable : MonoBehaviour
    {

        [Header("ToReset")]
        public bool transform_position;
        public bool transform_rotation;
        public bool transform_localScale;
        private Vector3 t_pos, t_scale;
        private Quaternion t_rot;

        public bool kill;

        public bool mesh;
        private Mesh o_mesh;

        // Start is called before the first frame update
        void Start()
        {
            Access.CheckPointManager().addResetable(this);
            save();
        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnDestroy()
        {
            CheckPointManager cpm = Access.CheckPointManager();
            if (!!cpm)
            cpm.removeResetable(this);
        }

        public void save()
        {
            if (transform_position)
            t_pos = transform.position;
            if (transform_rotation)
            t_rot = transform.rotation;
            if (transform_localScale)
            t_scale = transform.localScale;

            if (mesh)
            {
                MeshFilter MR = GetComponent<MeshFilter>();
                if (!!MR)
                {
                    o_mesh = MR.sharedMesh;
                }
                else
                {
                    this.LogError("Missing MeshRenderer On Object with Resetable");
                }
            }
        }

        public void load()
        {
            if (kill)
            Destroy(this.gameObject);

            if (transform_position)
            transform.position = t_pos;
            if (transform_rotation)
            transform.rotation = t_rot;
            if (transform_localScale)
            transform.localScale = t_scale;

            if (mesh)
            {
                MeshFilter MR = GetComponent<MeshFilter>();
                if (!!MR)
                {
                    MR.sharedMesh = o_mesh;
                }
                else
                {
                    this.LogError("Missing MeshRenderer On Object with Resetable");
                }
            }
        }

    }
}
