using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Schnibble;

public class BreakableObject : MonoBehaviour
{

    public GameObject brokenVersionRef;
    public float timeBeforeFragClean = 15f;
    public float breakDistance = 8f;
    public float breakSpeedThreshold = 30f;
    public float fragmentExplodeForce = 30f;

    private GameObject brokenVersionInst;
    private float elapsedTimeSinceBreak;
    private bool isBroken = false;

    public bool swallowBreak;
    public UnityEvent OnBreakFunc;

    // Start is called before the first frame update
    void Start()
    {
        elapsedTimeSinceBreak = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!!brokenVersionInst)
            elapsedTimeSinceBreak += Time.deltaTime;
        if (elapsedTimeSinceBreak >= timeBeforeFragClean)
        { Destroy(brokenVersionInst); Destroy(gameObject); }
    }

    void OnCollisionEnter(Collision iCol)
    {
        // if (!swallowBreak)
        // {
        //     //playerCollisionProc(iCol);
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
            if (OnBreakFunc!=null)
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
        CarController cc = iPC.car;
        // break cond : player speed > threshold speed && dist < breakdist
        if (cc.GetCurrentSpeed() < breakSpeedThreshold)
        { return; }

        float dist = Vector3.Distance(transform.position, cc.transform.position);
        if (dist > breakDistance)
        {  return; }


        Vector3 dir =  transform.position - cc.transform.position;
        
        RaycastHit rch;
        if (Physics.Raycast(transform.position, dir, out rch));
        {
            breakWall(rch.point);
            elapsedTimeSinceBreak = 0f;
            isBroken = true;
        }
    }

    private void playerCollisionProc(Collision iCol)
    {
        CarController cc = iCol.collider.gameObject.GetComponent<CarController>();
        if (!!Utils.isPlayer(iCol.collider.gameObject))
        {
            //Physics.IgnoreCollision(GetComponent<MeshCollider>(), iCol.collider);
            //Physics.IgnoreCollision(GetComponent<MeshCollider>(), Access.Player().gameObject.GetComponent<Collider>());

            breakWall(iCol.contacts[0].point);
            elapsedTimeSinceBreak = 0f;
        }
    }

    private void ballPowerCollisionProc(Collision iCol)
    {
        BallPowerObject BPO = iCol.collider.gameObject.GetComponent<BallPowerObject>();
        if (!!BPO)
        {
            Physics.IgnoreCollision(GetComponent<MeshCollider>(), iCol.collider);
            BPO.breakableObjectCollisionCorrection();

            breakWall(iCol.contacts[0].point);
            elapsedTimeSinceBreak = 0f;
        }   
    }

    private void breakWall(Vector3 contactPoint)
    {
        brokenVersionInst = GameObject.Instantiate(brokenVersionRef);

        brokenVersionInst.transform.position = transform.position;
        brokenVersionInst.transform.rotation = transform.rotation;

        MeshCollider mc = GetComponent<MeshCollider>();
        if (!!mc)
            mc.enabled = false;
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (!!mr)
            mr.enabled = false;

        List<Rigidbody> rbs = new List<Rigidbody>(brokenVersionInst.GetComponentsInChildren<Rigidbody>());
        foreach (Rigidbody rb in rbs)
        {
            Vector3 forceDir = contactPoint - rb.transform.position;
            rb.AddForce(forceDir.normalized * fragmentExplodeForce, ForceMode.Impulse);
            Debug.DrawRay(contactPoint, rb.transform.position * 10, Color.red);
        }
    }
}
