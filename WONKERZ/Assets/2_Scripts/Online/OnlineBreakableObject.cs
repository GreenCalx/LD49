using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Schnibble;
using Mirror;

public class OnlineBreakableObject : NetworkBehaviour
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


    public bool swallowBreak;
    public UnityEvent OnBreakFunc;

    [Header("Internals")]
    [SyncVar]
    private bool isBroken = false;

    // use PlayOnAwake SFX instead on spawned object
    //public AudioSource breakSFX;

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
            foreach(Transform child in brokenVersionInst.transform)
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
            OnlinePlayerController opc = iCol.gameObject.GetComponentInParent<OnlinePlayerController>();
            if (opc == null)
                return;

            CarController cc = opc.self_PlayerController.car;
            // break cond : player speed > threshold speed && dist < breakdist
            if (cc.GetCurrentSpeed() < breakSpeedThreshold)
            {
                return;
            }

            if (OnBreakFunc!=null)
            {
                if (!swallowBreak)
                { 
                    if (isServer)
                        BreakObject(opc);
                    else
                        opc.CmdBreakObject(this);

                }
                OnBreakFunc.Invoke();
            }
            else { 
                if (isServer)
                    BreakObject(opc); 
                else
                    opc.CmdBreakObject(this);
            }
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

    [Server]
    public void BreakObject(OnlinePlayerController iOPC)
    {
        // approximate contact point for explosion as collider position
        CarController cc = iOPC.self_PlayerController.car;
        BreakModelSwap(iOPC.self_PlayerController.rb.worldCenterOfMass, Mathf.Abs(cc.GetCurrentSpeed() / cc.maxTorque));
        elapsedTimeSinceBreak = 0f;
        isBroken = true;
    }

    //[ClientRpc]
    private void BreakModelSwap(Vector3 contactPoint, float forceMultiplier)
    {
        // instanciate broken object
        initialObject.transform.GetPositionAndRotation(out Vector3 localPosition, out Quaternion localRotation);
        // set position, ect in instanciate, or else it will be wrong when applying forces.

        brokenVersionInst = GameObject.Instantiate(brokenObjectRef, localPosition, localRotation, initialObject.transform.parent);
        NetworkServer.Spawn(brokenVersionInst);
        
        brokenVersionInst.transform.localScale = initialObject.transform.localScale;

        // remove initial object
        switch (initialObjectBreakingMode)
        {
            case BreakingMode.eDestroy: {
                GameObject.Destroy(initialObject);
            } break;
            case BreakingMode.eDeactivate: {
                initialObject.SetActive(false);
            } break;
            case BreakingMode.eDisableRenderer: {
                MeshCollider mc = initialObject.GetComponentInChildren<MeshCollider>();
                if (!!mc) mc.enabled = false;
                BoxCollider bc = initialObject.GetComponentInChildren<BoxCollider>();
                if (!!bc) bc.enabled = false;
                MeshRenderer mr = initialObject.GetComponentInChildren<MeshRenderer>();
                if (!!mr) mr.enabled = false;
            } break;
        }

        var rbs = brokenVersionInst.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rbs)
        {
            rb.AddForce(fragmentExplodeForce * forceMultiplier * (rb.worldCenterOfMass - (contactPoint - (upwardMultiplier*forceMultiplier)*transform.up)).normalized, ForceMode.VelocityChange);
            rb.AddTorque(fragmentExplodeForce * forceMultiplier * (rb.worldCenterOfMass - (contactPoint - (upwardMultiplier*forceMultiplier)*transform.up)).normalized, ForceMode.Impulse);
        }
    }

}
