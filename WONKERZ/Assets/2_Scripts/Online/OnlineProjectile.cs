using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
public class OnlineProjectile : NetworkBehaviour
{
    [Header("Tweaks")]
    public float lifeTime = 5f;
    public bool destroyOnDamageableCollision = true;
    public bool destroyIfNotMoving = true;
    public float minSpeedThreshold = 0.1f;
    [Header("Internal tweaks")]
    public float minTimeBeforeSelfKillChecks = 0.2f;
    private float elapsedLifeTime = 0f;
    private bool selfKillCalled = false;
    [Header("Internals")]
    public UnityEvent killCallback;
    private Rigidbody selfRB;

    void Start()
    {
        selfRB = GetComponent<Rigidbody>();
        selfKillCalled = false;
        elapsedLifeTime = 0f;

        if (destroyOnDamageableCollision)
        {
            OnlineDamager oDamager = GetComponent<OnlineDamager>();
            if (!!oDamager)
            {
                killCallback = new UnityEvent();
                killCallback.AddListener(KillObject);
                oDamager.OnDoDamageVoidCallbacks.Add(killCallback);
            }
        }
    }
    void Update()
    {
        if (selfKillCalled)
            return;

        if (elapsedLifeTime < lifeTime)
        {
            elapsedLifeTime += Time.deltaTime;
            if (elapsedLifeTime > minTimeBeforeSelfKillChecks)
            { KillIfNotMoving(); }



            return;
        }
        KillObject();
    }

    private void KillIfNotMoving()
    {
        if (!destroyIfNotMoving)
            return;
        if (selfKillCalled)
            return;

        if (!!selfRB)
        {
            if (selfRB.velocity.magnitude <=minTimeBeforeSelfKillChecks)
                KillObject();
        }
    }

    private void KillObject()
    {
        selfKillCalled = true;
        NetworkServer.Destroy(gameObject);
    }
}
