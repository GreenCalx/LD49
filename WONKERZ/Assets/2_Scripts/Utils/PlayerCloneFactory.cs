using System;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Schnibble;

namespace Wonkerz {

    public sealed class PlayerCloneFactory
    {
        public IEnumerator SpawnOnlineClone(Transform iParent = null, UnityEvent iCallback = null)
        {
            GameObject clone = GetOnlineClone();
            while (clone==null)
            {
                clone = GetOnlineClone();
                yield return null;
            }
            //clone.transform.parent = iParent;
            //yield return null; // wait 1 frame for component destruction
            clone.SetActive(true);
            if (iCallback!=null)
            iCallback.Invoke();
        }

        public readonly System.Type[] FILTER_0 = 
            {   
                typeof(DeathController),
                typeof(PlayerController), 
                typeof(SchCarController),
                typeof(TrickTracker),
                typeof(PowerController),
                typeof(AudioSource),
                typeof(AudioListener),
                typeof(ParticleSystem),
                typeof(Light),
                typeof(MotorSoundRenderer),
                typeof(PlayerCarProcAnimator)
            };

        private PlayerCloneFactory() {}
        private static PlayerCloneFactory instance;
        public static PlayerCloneFactory GetInstance()
        {
            if (instance==null)
            {
                instance = new PlayerCloneFactory();
            }
            return instance;
        }

        // ---

        private GameObject GetOnlineClone()
        {
            if (NetworkRoomManagerExt.singleton.onlineGameManager==null)
                return null;
            if (NetworkRoomManagerExt.singleton.onlineGameManager.localPlayer==null)
                return null;

            GameObject root_clone = new GameObject();
            root_clone.name = "DeathClone";
            //root_clone.transform.position = NetworkRoomManagerExt.singleton.onlineGameManager.localPlayer.
            root_clone.SetActive(false);
            RecursiveDeepClone(root_clone.transform, NetworkRoomManagerExt.singleton.onlineGameManager.localPlayer.transform);
            //ExplodeChildBodies ecb = root_clone.AddComponent<ExplodeChildBodies>();
            root_clone.SetActive(true);
            //ecb.triggerExplosion();
            return root_clone;
        }

        private void RecursiveDeepClone(Transform iCloneRoot, Transform iDeepCloneTarget)
        {
            foreach(Transform child in iDeepCloneTarget)
            {
                // skip debug models
                if (child.gameObject.tag == Constants.TAG_DBG)
                    continue;

                GameObject carPartClone = new GameObject();
                carPartClone.name = child.gameObject.name;
                TryDecorateOnlineDeathClone(ref carPartClone, child, iCloneRoot);

                if (child.childCount > 0)
                    RecursiveDeepClone(carPartClone.transform, child);
            }
        }

        private void TryDecorateOnlineDeathClone(ref GameObject iToDecorate, Transform iChildToCopy, Transform iRootTransform)
        {
            iToDecorate.transform.parent        = iRootTransform;
            iToDecorate.transform.localPosition = iChildToCopy.localPosition;
            iToDecorate.transform.localRotation = iChildToCopy.localRotation;
            
            MeshRenderer mr = iChildToCopy.GetComponent<MeshRenderer>();
            MeshFilter   mf = iChildToCopy.GetComponent<MeshFilter>();

            // skip a part with no render
            if ( (mf==null) || (mr==null) || !mr.enabled)
                return;

            MeshFilter cloned_mf = iToDecorate.AddComponent(typeof(MeshFilter)) as MeshFilter;
            cloned_mf.sharedMesh = mf.sharedMesh;

            MeshRenderer cloned_mr = iToDecorate.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
            cloned_mr.sharedMaterials = mr.sharedMaterials;

            Rigidbody rb = iToDecorate.gameObject.AddComponent<Rigidbody>();
            MeshCollider mc = iToDecorate.gameObject.AddComponent<MeshCollider>();
            mc.convex    = true;

            iToDecorate.gameObject.layer = LayerMask.NameToLayer(Constants.LYR_NOPLAYERCOL);
        }

        // -----------------------------------------
        // SOLO PLAYER LEGACY
        public IEnumerator SpawnPhysxClone(Transform iParent = null, UnityEvent iCallback = null)
        {
            GameObject clone = GetPhysxClone();
            clone.transform.parent = iParent;
            yield return null; // wait 1 frame for component destruction
            clone.SetActive(true);
            if (iCallback!=null)
            iCallback.Invoke();
        }
        private GameObject GetPhysxClone()
        {
            // Player Ref
            GameObject player = Access.Player().gameObject;

            // Create empty root
            GameObject clone = new GameObject();
            // clone.transform.position = player.transform.position;
            // clone.transform.rotation = player.transform.rotation;
            // clone.transform.localScale = player.transform.localScale;
            clone.name = "DeathClone";
            clone.SetActive(false);
            // ---

            // Create PlayerClone under root
            GameObject pClone = GameObject.Instantiate(player, clone.transform);
            pClone.SetActive(false);

            // Recursive clean of clone based on filter
            // Destroy uneedeed components
            filter(ref pClone, FILTER_0);

            // Decorate remaining
            decorateMeshFilters(ref pClone);

            // Activate player clone
            pClone.SetActive(true);
        
            return clone;
        }

        private void decorateMeshFilters(ref GameObject iToDecorate)
        {
            // Decorate self
            // Deco constraint : MeshFilter
            MeshFilter mf = iToDecorate.GetComponent<MeshFilter>();
            if (mf!=null)
            {
                //this.Log("decorate " + iToDecorate.name);
                // Deco0 : Add Rigidbody
                Rigidbody rb = iToDecorate.gameObject.GetComponent<Rigidbody>();
                if (rb==null)
                rb = iToDecorate.gameObject.AddComponent<Rigidbody>();

                // Deco1 : Add Collider
                MeshCollider mc = iToDecorate.gameObject.AddComponent<MeshCollider>();
                mc.convex    = true;

                // Deco2 : Change Collision Layer
                iToDecorate.gameObject.layer = LayerMask.NameToLayer(Constants.LYR_NOPLAYERCOL);
            }

            iToDecorate.gameObject.SetActive(true);

            // Call decorate on children, if exists
            foreach (Transform child in iToDecorate.transform)
            {
                GameObject child_go = child.gameObject;
                decorateMeshFilters(ref child_go);
            }

        }

        private void filter(ref GameObject iToFilter, System.Type[] iFilter)
        {
            int n_types = iFilter.Length;
     
            // Filter self
            var comps = iToFilter.GetComponents(typeof(Component));
            foreach( var c in comps)
            {
                for ( int i=0; i<n_types ; i++)
                {
                    Component[] typeComps;
                    typeComps = c.GetComponents(iFilter[i]);
                    for (int j=0; j < typeComps.Length ; j++)
                    { 
                        // Disabling behaviors to ensure no Awake()
                        // slips thru on SetActive(true) later on.
                        try {
                            Behaviour b = (Behaviour)typeComps[j];
                            if (!!b)
                            { 
                                b.enabled = false; 
                                //this.Log( "- disabling : " +  b.name); 
                            }
                            #pragma warning disable CS0168
                        } catch( InvalidCastException ice ) 
                    { /* expected on pure components */}

                        GameObject.Destroy(typeComps[j]); 
                    }
                    #pragma warning restore CS0168
                }
            }
        
            // Call filter on children, if exists
            foreach (Transform child in iToFilter.transform)
            {
                //this.Log("filtering " + child.gameObject.name);
                GameObject child_go = child.gameObject;
                filter(ref child_go, iFilter);
            }


        }

    }
}
