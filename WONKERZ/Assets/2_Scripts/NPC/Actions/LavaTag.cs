using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Wonkerz {

    //[RequireComponent(typeof(MeshRenderer))]
    public class LavaTag : MonoBehaviour
    {
        private Material[] m_prevMaterials;
        MeshRenderer MR;
        private bool isEnabled = false;
        private bool inited = false;
        // Start is called before the first frame update
        void Start()
        {
            init();
        }

        private void init()
        {
            if (inited)
            return;

            if (GetComponents<LavaTag>().Length > 1)
            {
                disable();
                return;
            }

            MR = GetComponent<MeshRenderer>();
            if (MR==null)
            MR = GetComponentInChildren<MeshRenderer>();
            inited = true;
        }

        public void OnCollisionEnter(Collision iCol)
        {
            killIfPlayer(iCol);
        }

        public void OnCollisionStay(Collision iCol)
        {
            killIfPlayer(iCol);
        }

        private void killIfPlayer(Collision iCol)
        {
            if (!isEnabled)
            return;

            if (Utils.collisionIsPlayer(iCol))
            {
                Access.Player().Kill();
                disable();
            }
        }

        public void enable(Material lavaMaterial)
        {
            init();

            if (!!MR)
            {
                m_prevMaterials =  MR.sharedMaterials;
                int n_mats = MR.sharedMaterials.Length;

                List<Material> lavaMaterials = new List<Material>(0);
                for(int i=0; i < n_mats ; i++)
                {
                    lavaMaterials.Add(lavaMaterial);
                }
                MR.sharedMaterials = lavaMaterials.ToArray();
            }

            isEnabled = true;
        }

        public void disable()
        {
            isEnabled = false;
            if (!!MR)
            {
                MR.sharedMaterials = m_prevMaterials;
                Destroy(this);
            }
        }
    }
}
