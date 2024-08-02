using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Schnibble;
using Wonkerz;
using Mirror;

public class OnlineBreakableObject : NetworkBehaviour
{
    [Header("Prefab Mands")]
    public GameObject brokenObjectRef;
    public GameObject collectiblePatchPrefab;
    [Header("Self Refs")]
    public OnlineDamageable self_DamageableRef;
    
    public bool destroyParentGameObject = true;
    public float timeBeforeFragClean = 15f;

    // Note : It is not armor
    // this threshold won't reduce incoming damage
    // Damages either pass or no pass
    // So when the player is strong he crushes
    public float damageThreshold = 30f;
    [SyncVar]
    public int maxHP = 300;
    public float fragmentExplodeForce = 30f;
    public float upwardMultiplier = 1f;
    public bool swallowBreak;
    public UnityEvent OnPreBreakFunc;
    public UnityEvent OnPostBreakFunc;
    
    [Header("Internals")]
    [SyncVar]
    public int currHP;
    [SyncVar]
    private bool isBroken = false;
   
    private GameObject brokenVersionInst;
    // private const float damageCooldown = 0.2f;
    // private float localElapsedTime;
    private int damageIndex = 0;
    private Material selfMat;


    // use PlayOnAwake SFX instead on spawned object
    //public AudioSource breakSFX;

    // Start is called before the first frame update
    void Start()
    {
        currHP = maxHP;
        // localElapsedTime = damageCooldown + 1f;

        damageIndex = 0;
        MeshRenderer mr = GetComponentInChildren<MeshRenderer>();
        selfMat = mr.material;
    }

    // Update is called once per frame
    void Update()
    {
        // if (localElapsedTime < damageCooldown)
        // {
        //     localElapsedTime += Time.deltaTime;
        // }
    }

    void OnCollisionEnter(Collision iCol)
    {
        //OnBreak(iCol.collider);
    }


    // private void OnBreak(Collider iCol)
    // {
    //     if (isBroken)
    //         return;

    //     // Collider is player
    //     if (!!Wonkerz.Utils.colliderIsPlayer(iCol))
    //     {
    //         OnlinePlayerController opc = iCol.gameObject.GetComponentInParent<OnlinePlayerController>();
    //         if (opc == null)
    //             return;
    //         if (!opc.isLocalPlayer)
    //             return;

    //         WkzCar cc = opc.self_PlayerController.car.GetCar();
    //         // break cond : player speed > threshold speed && dist < breakdist
    //         if (cc.GetCurrentSpeedInKmH() < breakSpeedThreshold)
    //         {
    //             return;
    //         }

    //         // break cond 2 : object hp must bo <=0
    //         if (currHP > 0)
    //         {
    //             int damage = (int) Mathf.Abs((float)cc.GetCurrentSpeedInKmH());
    //             if (isServer)
    //                 TryDoDamage(damage);
    //             else
    //                 CmdTryDoDamage(damage);

    //             if (currHP > 0)
    //                 return; // HPs still above 0 after damage, we exit
    //         }

    //         if (OnBreakFunc!=null)
    //         {
    //             if (!swallowBreak)
    //             { 
    //                 if (isServer)
    //                     BreakObject(opc);
    //                 else
    //                     opc.CmdBreakObject(this);
    //             }
    //             OnBreakFunc.Invoke();
    //         }
    //         else { 
    //             if (isServer)
    //                 BreakObject(opc); 
    //             else
    //                 opc.CmdBreakObject(this);
    //         }
    //     }
    //     // Or Player Damager (player powers, enemies...)
    //     else if (!!iCol.gameObject.GetComponent<OnlineDamager>())
    //     {
    //         OnlineDamager od = iCol.gameObject.GetComponent<OnlineDamager>();
    //         if (currHP > 0)
    //         {
    //             if (isServer)
    //                 TryDoDamage(od.damage);
    //             else
    //                 CmdTryDoDamage(od.damage);

    //             if (currHP > 0)
    //                 return; // HPs still above 0 after damage, we exit
    //         }
    //         if (OnBreakFunc!=null)
    //         {
    //             if (!swallowBreak)
    //             { 
    //                 if (isServer)
    //                     BreakObjectFromDamager(od);
    //                 else
    //                     od.CmdBreakObject(this);
    //             }
    //             OnBreakFunc.Invoke();
    //         }
    //         else { 
    //             if (isServer)
    //                 BreakObjectFromDamager(od); 
    //             else
    //                 od.CmdBreakObject(this);
    //         }
    //     }
    // }

    // void OnTriggerEnter(Collider iCol)
    // {
    //     OnBreak(iCol);
    // }

    // void OnTriggerStay(Collider iCol)
    // {
    //     OnBreak(iCol);
    // }

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

    // [Server]
    // public void BreakObject(OnlinePlayerController iOPC)
    // {
    //     if (isBroken)
    //         return;
    //     isBroken = true;

    //     // approximate contact point for explosion as collider position
    //     WkzCar cc = iOPC.self_PlayerController.car.GetCar();

    //     if (isServerOnly)
    //         disableSelfColliders();
    //     RpcDisableSelfColliders();

    //     if (isServerOnly)
    //         BreakModelSwap(iOPC.self_PlayerController.GetRigidbody().worldCenterOfMass, Mathf.Abs((float)cc.GetCurrentSpeedInKmH() / (float)cc.mutDef.maxSpeedInKmH));
    //     RpcBreakModelSwap(iOPC.self_PlayerController.GetRigidbody().worldCenterOfMass, Mathf.Abs((float)cc.GetCurrentSpeedInKmH() / (float)cc.mutDef.maxSpeedInKmH));

    //     StartCoroutine(DeleteInitialCrateCo());
    // }

    [Server]
    public void BreakObject(OnlineDamageSnapshot iDamageSnap)
    {
        if (isBroken)
            return;

        if (OnPreBreakFunc!=null)
            OnPreBreakFunc.Invoke();

        isBroken = true;
        if (swallowBreak)
        {
            if (OnPostBreakFunc!=null)
                OnPostBreakFunc.Invoke();
            return;
        }

        float forceMul = Mathf.Clamp( iDamageSnap.ownerVelocity.magnitude, 1f, 100f);
        Vector3 damagerPos  = iDamageSnap.worldOrigin;
        Vector3 damagerDir  = iDamageSnap.ownerVelocity.normalized;    

        // approximate contact point for explosion as collider position
        if (isServerOnly)
            disableSelfColliders();
        RpcDisableSelfColliders();

        if (isServerOnly)
            BreakModelSwap(damagerPos, forceMul);
        RpcBreakModelSwap(damagerPos, forceMul);

        StartCoroutine(DeleteInitialCrateCo());

        if (OnPostBreakFunc!=null)
            OnPostBreakFunc.Invoke();
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

    [ClientRpc]
    private void RpcUpdateImpactGFX()
    {
        UpdateImpactGFX();
    }
    
    private void UpdateImpactGFX()
    {
        MeshRenderer mr = GetComponentInChildren<MeshRenderer>();
        int hp_chunk = maxHP / 3; // size of impact damage atlas

        if (currHP == maxHP) { damageIndex = 0;}
        else if (currHP > (maxHP - hp_chunk)) { damageIndex = 1;}
        else if (currHP > (maxHP - hp_chunk*2)) { damageIndex = 2;}
        else damageIndex = 3;

        selfMat.SetInteger("_DamageTexID", damageIndex);
        
    }

    [Command]
    private void CmdTryTakeDamage(OnlineDamageSnapshot iDamageSnap)
    {
        TakeDamage(iDamageSnap);
    }

    [Server]
    public void TakeDamage(OnlineDamageSnapshot iDamageSnap)
    {
        // if (localElapsedTime < damageCooldown) // invulnerability CD
        // { return; }
        // localElapsedTime = 0f; // do damage, reset local cd

        // damage
        //this.Log("Damage to crate : " + iIncomingDamage);

        
        if (currHP > 0)
        {
            currHP -= iDamageSnap.damage;
            currHP = Mathf.Clamp(currHP, 0, maxHP);
        }
        RpcUpdateImpactGFX();
        if (currHP > 0)
            return;

        // HP at 0, break object !
        BreakObject(iDamageSnap);
    }

}
