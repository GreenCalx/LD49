using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Mirror;
using Wonkerz;

public class OnlineDamageable : NetworkBehaviour
{
    public GameObject owner;

    [Header("Tweaks")]
    public List<UnityEvent<OnlineDamageSnapshot>> OnTakeDamageCallbacks;
    private readonly float delayBetweenDamages = 0.2f;
    private float elapsedTimeSinceLastDamage = 0f;
    
    void Start()
    {
        elapsedTimeSinceLastDamage = delayBetweenDamages;
    }

    void FixedUpdate()
    {
        if (elapsedTimeSinceLastDamage < delayBetweenDamages)
            elapsedTimeSinceLastDamage += Time.fixedDeltaTime;
    }

    public bool TryTakeDamage(OnlineDamageSnapshot iDamageSnap)
    {
        if (elapsedTimeSinceLastDamage < delayBetweenDamages)
            return false;
        
        if (iDamageSnap.owner == owner)
            return false; // self damage

        OnTakeDamageCallbacks.ForEach(e => e.Invoke(iDamageSnap));

        elapsedTimeSinceLastDamage = 0f;
        return true;        
    }

}
