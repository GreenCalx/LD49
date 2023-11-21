using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;

public class FallingObject : MonoBehaviour
{
    public GameObject ParticleSystem_OnImpact;
    public int MAX_ParticleSystems = 5;
    public float delayedDestroyOnPS = 5f;
    public float minDistBetweenPS = 5f;

    private Queue<ParticleSystem> inst_PS;

    public float timeBeforeCollisionCallbacks;
    private float elapsedTimeBeforeCollisionCallbacks;

    public float timeBeforeStoppingAll;
    private float elapsedTimeBeforeStoppingAll;
    [HideInInspector]
    public bool isFalling = false; // changed by FallingObjectTrigger
    private bool delayedStartComplete   = false;
    private bool shouldStopAll          = false;

    void Start()
    {
        elapsedTimeBeforeCollisionCallbacks = 0f;
        elapsedTimeBeforeStoppingAll        = 0f;
        isFalling = false;
        delayedStartComplete = false;
        shouldStopAll        = false;
        inst_PS = new Queue<ParticleSystem>(0);
    }

    void Update()
    {
        if (isFalling && !delayedStartComplete)
        {
            elapsedTimeBeforeCollisionCallbacks += Time.deltaTime;
            delayedStartComplete = (elapsedTimeBeforeCollisionCallbacks >= timeBeforeCollisionCallbacks);
        }
        if (isFalling && !shouldStopAll)
        {
            elapsedTimeBeforeStoppingAll += Time.deltaTime;
            shouldStopAll = (elapsedTimeBeforeStoppingAll >= timeBeforeStoppingAll);
        }
    }

    void OnCollisionEnter(Collision iCollision)
    {
        if (!isFalling || !delayedStartComplete)
            return;

        //if (iCollision.gameObject.GetComponent<Ground>()==null)
        //return;

        effectOnCollision(iCollision);
    }

    void OnCollisionStay(Collision iCollision)
    {
        if (!isFalling || !delayedStartComplete)
            return;
        
        //if (iCollision.gameObject.GetComponent<Ground>()==null)
        //return;
        
        if (!shouldStopAll)
        { return; }

        stopObject();
    }

    private void effectOnCollision(Collision iCollision)
    {
        if (!!ParticleSystem_OnImpact)
        {
            //if (iCollision.gameObject.GetComponent<Ground>()==null)
            //return;

            List<ContactPoint> contacts = new List<ContactPoint>();
            int n_contacts = iCollision.GetContacts(contacts);
            for (int i=0;i<n_contacts;i++)
            {
                ContactPoint cp = contacts[i];
                if (cp.otherCollider==null)
                    continue;

                bool skip_point = false;
                foreach (ParticleSystem ips in inst_PS)
                {
                    if (Vector3.Distance(ips.transform.position, cp.point) <= minDistBetweenPS)
                    {
                        this.Log("Point skipped!");
                        skip_point = true; break;
                        
                    }
                }
                if (skip_point)
                    continue;

                if (inst_PS.Count >= MAX_ParticleSystems)
                {
                    ParticleSystem to_rm = inst_PS.Dequeue();
                    to_rm.Stop();
                    Destroy(to_rm.gameObject, delayedDestroyOnPS);
                }

                GameObject ps_go = Instantiate(ParticleSystem_OnImpact, transform);
                ps_go.transform.position = cp.point;
                ParticleSystem ps = ps_go.GetComponent<ParticleSystem>();
                if (!!ps)
                {
                    ps.Play();
                    inst_PS.Enqueue(ps);
                }
            }
        }
    }

    private void stopObject()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb!=null)
        {
            Destroy(rb);
            while(inst_PS.Count > 0)
            {
                ParticleSystem ps = inst_PS.Dequeue();
                ps.Stop();
                Destroy(ps,delayedDestroyOnPS);
            }
        }

        PlayerDamager pd = GetComponent<PlayerDamager>();
        if (pd!=null)
        {
            Destroy(pd);
        }
    }
}
