using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Schnibble;

namespace Wonkerz
{

    public class BreakableObject : MonoBehaviour
    {
        public enum BreakingMode
        {
            eDestroy,
            eDeactivate,
            eDisableRenderer
        }
        // NOTE initialObject should be the gameobject having the renderer.
        public GameObject initialObject;
        public BreakingMode initialObjectBreakingMode;

        public GameObject brokenObjectRef;

        public bool destroyParentGameObject = true;

        public float timeBeforeFragClean = 15f;

        public float breakSpeedThreshold = 30f;
        public float fragmentExplodeForce = 30f;
        public float upwardMultiplier = 1f;

        private GameObject brokenVersionInst;
        private float elapsedTimeSinceBreak;
        private bool isBroken = false;

        public bool swallowBreak;
        public UnityEvent OnBreakFunc;

        public AudioSource breakSFX;

        // Start is called before the first frame update
        void Start()
        {
            elapsedTimeSinceBreak = 0f;
        }

        // Update is called once per frame
        void Update()
        {
            if (!!brokenVersionInst) elapsedTimeSinceBreak += Time.deltaTime;
            if (elapsedTimeSinceBreak >= timeBeforeFragClean)
            {
                foreach (Transform child in brokenVersionInst.transform)
                {
                    Destroy(child.gameObject);
                }
                Destroy(brokenVersionInst);
                if (destroyParentGameObject)
                Destroy(gameObject);
            }
        }

        void OnCollisionEnter(Collision iCol)
        {
            // if (!swallowBreak)
            // {
            //     playerCollisionProc(iCol);
            // }
            // else if (OnBreakFunc!=null)
            // {
            //     OnBreakFunc.Invoke();
            // }
        }

        private void OnBreak(Collider iCol)
        {
            if (isBroken)
            return;

            if (!!Utils.colliderIsPlayer(iCol))
            {
                if (OnBreakFunc != null)
                {
                    if (!swallowBreak)
                    { tryBreak(Access.Player()); }
                    OnBreakFunc.Invoke();
                }
                else { tryBreak(Access.Player()); }
            }
        }

        void OnTriggerEnter(Collider iCol)
        {
            OnBreak(iCol);
        }

        void OnTriggerStay(Collider iCol)
        {
            OnBreak(iCol);
        }

        private void tryBreak(PlayerController iPC)
        {
            SchCar car = iPC.car.car;
            // break cond : player speed > threshold speed && dist < breakdist
            var carSpeed = car.GetCurrentSpeedInKmH();
            if (carSpeed < breakSpeedThreshold)
            { return; }

            // approximate contact point for explosion as collider position
            // TODO: Compute somewhat physically accurate collisions?
            var forceMultiplier = Mathf.Clamp01((float)carSpeed / 300.00f);
            breakWall(car.GetChassis().GetBody().worldCenterOfMass, forceMultiplier);

            elapsedTimeSinceBreak = 0f;
            Schnibble.Utils.SpawnAudioSource( breakSFX, transform);
            isBroken = true;

            //Vector3 dir =  transform.position - cc.transform.position;
            //
            //RaycastHit rch;
            //if (Physics.Raycast(transform.position, dir, out rch))
            //{
            //breakWall(rch.point, cc.GetCurrentSpeed() / cc.maxTorque);
            //elapsedTimeSinceBreak = 0f;
            //// SFX
            //Schnibble.Utils.SpawnAudioSource( breakSFX, transform);
            //isBroken = true;
            //}
        }

        private void playerCollisionProc(Collision iCol)
        {
            #if false
            CarController cc = iCol.collider.gameObject.GetComponent<CarController>();
            if (!!Utils.isPlayer(iCol.collider.gameObject))
            {
                breakWall(iCol.GetContact(0).point, Mathf.Lerp(0.0f, 1.0, ));
                elapsedTimeSinceBreak = 0f;
            }
            #endif
        }

        private void breakWall(Vector3 contactPoint, float forceMultiplier)
        {
            // instanciate broken object
            initialObject.transform.GetPositionAndRotation(out Vector3 localPosition, out Quaternion localRotation);
            // set position, ect in instanciate, or else it will be wrong when applying forces.
            brokenVersionInst = GameObject.Instantiate(brokenObjectRef, localPosition, localRotation, initialObject.transform.parent);
            brokenVersionInst.transform.localScale = initialObject.transform.localScale;

            // remove initial object
            switch (initialObjectBreakingMode)
            {
                case BreakingMode.eDestroy:
                    {
                        GameObject.Destroy(initialObject);
                    }
                    break;
                case BreakingMode.eDeactivate:
                    {
                        initialObject.SetActive(false);
                    }
                    break;
                case BreakingMode.eDisableRenderer:
                    {
                        MeshCollider mc = initialObject.GetComponentInChildren<MeshCollider>();
                        if (!!mc) mc.enabled = false;
                        BoxCollider bc = initialObject.GetComponentInChildren<BoxCollider>();
                        if (!!bc) bc.enabled = false;
                        MeshRenderer mr = initialObject.GetComponentInChildren<MeshRenderer>();
                        if (!!mr) mr.enabled = false;
                    }
                    break;
            }

            var rbs = brokenVersionInst.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in rbs)
            {
                rb.AddForce(fragmentExplodeForce * forceMultiplier * (rb.worldCenterOfMass - (contactPoint - (upwardMultiplier * forceMultiplier) * transform.up)).normalized, ForceMode.VelocityChange);
            }
        }
    }
}
