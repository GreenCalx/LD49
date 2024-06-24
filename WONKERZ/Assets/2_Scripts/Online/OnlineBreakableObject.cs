using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Schnibble;
using Wonkerz;
using Mirror;

public class OnlineBreakableObject : NetworkBehaviour
{
    public GameObject brokenObjectRef;
    public GameObject collectiblePatchPrefab;
    public bool destroyParentGameObject = true;
    public float timeBeforeFragClean = 15f;

    public float breakSpeedThreshold = 30f;
    public float fragmentExplodeForce = 30f;
    public float upwardMultiplier = 1f;
    public bool swallowBreak;
    public UnityEvent OnBreakFunc;

    [Header("Internals")]
    [SyncVar]
    private bool isBroken = false;
   
    private GameObject brokenVersionInst;

    // use PlayOnAwake SFX instead on spawned object
    //public AudioSource breakSFX;

    // Start is called before the first frame update
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {

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

        if (!!Wonkerz.Utils.colliderIsPlayer(iCol))
        {
            OnlinePlayerController opc = iCol.gameObject.GetComponentInParent<OnlinePlayerController>();
            if (opc == null)
                return;
            if (!opc.isLocalPlayer)
                return;

            WkzCar cc = opc.self_PlayerController.car.GetCar();
            // break cond : player speed > threshold speed && dist < breakdist
            if (cc.GetCurrentSpeedInKmH() < breakSpeedThreshold)
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

    public void disableSelfColliders()
    {
        BoxCollider bc = GetComponent<BoxCollider>();
        if (!!bc) 
            bc.enabled = false;
        MeshCollider mc = GetComponent<MeshCollider>();
        if (!!mc) 
            mc.enabled = false;
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (!!mr) 
            mr.enabled = false;

        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            bc = child.gameObject.GetComponent<BoxCollider>();
            if (!!bc) 
                bc.enabled = false;
            
            mc = child.gameObject.GetComponent<MeshCollider>();
            if (!!mc) 
                mc.enabled = false;
            
            mr = child.GetComponent<MeshRenderer>();
            if (!!mr) 
                mr.enabled = false;            
        }
    }

    [Server]
    public void BreakObject(OnlinePlayerController iOPC)
    {
        if (isBroken)
            return;
        isBroken = true;

        // approximate contact point for explosion as collider position
        WkzCar cc = iOPC.self_PlayerController.car.GetCar();

        if (isServerOnly)
            disableSelfColliders();
        RpcDisableSelfColliders();

        if (isServerOnly)
            BreakModelSwap(iOPC.self_PlayerController.GetRigidbody().worldCenterOfMass, Mathf.Abs((float)cc.GetCurrentSpeedInKmH() / (float)cc.motor.def.maxTorque));
        RpcBreakModelSwap(iOPC.self_PlayerController.GetRigidbody().worldCenterOfMass, Mathf.Abs((float)cc.GetCurrentSpeedInKmH() / (float)cc.motor.def.maxTorque));

        StartCoroutine(DeleteInitialCrateCo());
    }

    [Server]
    IEnumerator DeleteInitialCrateCo()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    [ClientRpc]
    private void RpcDisableSelfColliders()
    {
        disableSelfColliders();
    }

    [ClientRpc]
    private void RpcBreakModelSwap(Vector3 contactPoint, float forceMultiplier)
    {
        BreakModelSwap(contactPoint, forceMultiplier);
    }

    private void BreakModelSwap(Vector3 contactPoint, float forceMultiplier)
    {
        // As its called by server which handles destruction
        // isBroken should be true
        if (!isBroken)
            return;

        // instanciate broken object
        transform.GetPositionAndRotation(out Vector3 localPosition, out Quaternion localRotation);
        // set position, ect in instanciate, or else it will be wrong when applying forces.

        brokenVersionInst = Instantiate(brokenObjectRef, localPosition, localRotation);
        brokenVersionInst.transform.parent = null;
        brokenVersionInst.transform.localScale = transform.localScale;

        NetworkServer.Spawn(brokenVersionInst);
        
        ExplodeChildBodies ECB = brokenVersionInst.GetComponent<ExplodeChildBodies>();
        if (!!ECB)
        {
            Vector3 dir = (contactPoint - ((upwardMultiplier*forceMultiplier)*transform.up)).normalized;
            ECB.setExplosionDir(dir, fragmentExplodeForce);
            ECB.triggerExplosion();
        }

        GameObject collectPatch = Instantiate(collectiblePatchPrefab, localPosition, localRotation);
        NetworkServer.Spawn(collectPatch);
    }

}
