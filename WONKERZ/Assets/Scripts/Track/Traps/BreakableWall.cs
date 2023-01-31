using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableWall : MonoBehaviour
{

    public GameObject brokenVersionRef;
    public float timeBeforeFragClean = 15f;

    private GameObject brokenVersionInst;
    private float elapsedTimeSinceBreak;
    

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
        foreach(Rigidbody rb in rbs)
        {
            Vector3 forceDir = contactPoint - rb.transform.position;
            rb.AddForce( forceDir * 30f, ForceMode.Impulse);
            Debug.DrawRay(contactPoint, rb.transform.position * 10, Color.red);
        }
    }
}
