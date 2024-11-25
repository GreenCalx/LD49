using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Mirror;
using Wonkerz;

public class OnlineDamageSnapshot
{
    public GameObject owner; // netid for online ? make offlinedamagesnapshot ?
    public int damage;
    public int repulsionForce;
    public Vector3 worldOrigin;
    public Vector3 ownerVelocity;
    

    public OnlineDamageSnapshot()
    {
        owner = null;
        damage = 0;
        repulsionForce = 1;
        worldOrigin = Vector3.zero;
        ownerVelocity = Vector3.zero;
    }

    public OnlineDamageSnapshot(OnlineDamager iDamager)
    {
        owner       = iDamager.gameObject;
        damage      = iDamager.damage;
        repulsionForce = iDamager.optional_repulsion_force;

        worldOrigin = iDamager.transform.position;
        
        Rigidbody rb = iDamager.GetComponent<Rigidbody>();
        ownerVelocity = (!!rb) ? rb.velocity : Vector3.zero;
    }
}

[System.Serializable]
public class DamageEvent : UnityEvent<int>
{
}

public class OnlineDamager : NetworkBehaviour
{
    public GameObject owner;
    [Header("Can do damage to:")]
    public bool DoDamageToPlayers = true;
    public bool DoDamageToObjects = true;
    public bool DoDamageToEnemies = true;

    [Header("Damage Detection triggers")]
    public bool FromCollision = true;
    public bool FromTriggers = false;
    public bool FromParticles = false;
    public bool FirstContactOnly = true;
    [Header("Filters")]
    public List<OnlineDamageable> filteredOutDamageables = new List<OnlineDamageable>();

    [Header("Tweaks")]
    public int damage = 99;
    [Range(-500,500)]
    public int optional_repulsion_force = 0;
    public List<DamageEvent> OnDoDamageCallbacks;
    public List<UnityEvent> OnDoDamageVoidCallbacks;
    private readonly float delayBetweenDamages = 0.2f;
    private float elapsedTimeSinceLastDamage = 0f;
    
    void Start()
    {
        elapsedTimeSinceLastDamage = delayBetweenDamages;
    }

    void FixedUpdate()
    {
        elapsedTimeSinceLastDamage += Time.fixedDeltaTime;
    }

    void OnCollisionEnter(Collision iCol)
    {
        if (!FromCollision)
            return;

        ContactPoint cp = iCol.contacts[0];
        TryDoDamage(iCol.collider.gameObject, cp.point, cp.normal);
    }

    void OnTriggerEnter(Collider iCol)
    {
        if (!FromTriggers)
            return;

        TryDoDamage(iCol.gameObject, transform.position, transform.forward);
    }

    void OnParticleCollision(GameObject other)
    {
        if (!FromParticles)
            return;
 
        TryDoDamage(other, transform.position, transform.forward);
    }

    void OnCollisionStay(Collision iCol)
    {
        if (!FromCollision || FirstContactOnly)
            return;

        ContactPoint cp = iCol.contacts[0];
        TryDoDamage(iCol.collider.gameObject, cp.point, cp.normal);
    }

    void OnTriggerStay(Collider iCol)
    {
        if (!FromTriggers || FirstContactOnly)
            return;

        TryDoDamage(iCol.gameObject, transform.position, transform.forward);
    }

    private bool TryDoDamage(GameObject iDamageTarget, Vector3 iDamageOrigin, Vector3 iDamageDir)
    {
        if (elapsedTimeSinceLastDamage < delayBetweenDamages)
            return false;

        OnlineDamageable oDamageable = iDamageTarget.GetComponent<OnlineDamageable>();
        if (oDamageable==null)
        {
            oDamageable = iDamageTarget.GetComponentInParent<OnlineDamageable>();
            if (oDamageable==null)
                return false; 
        }
        if (filteredOutDamageables.Contains(oDamageable))
            return false;

        OnlineDamageSnapshot snapshot = MakeSnapshot();
        bool did_damage = false;
        if (DoDamageToPlayers)
        {
            if (Wonkerz.Utils.isOnlinePlayer(oDamageable.owner))
            {
                did_damage = oDamageable.TryTakeDamage(snapshot);
            }
        }

        if (DoDamageToObjects)
        {
            OnlineBreakableObject obo = oDamageable.owner.GetComponent<OnlineBreakableObject>();
            if (!!obo)
            {
                did_damage = oDamageable.TryTakeDamage(snapshot);
            }
        }

        if (DoDamageToEnemies)
        {
            if (!!oDamageable.owner.GetComponent<WkzEnemy>())
            {
                did_damage = oDamageable.TryTakeDamage(snapshot);
            }
        }
        
        if (!did_damage)
        { return false;}

        if (OnDoDamageVoidCallbacks!=null)
            OnDoDamageVoidCallbacks.ForEach(e => e.Invoke());
        if (OnDoDamageCallbacks!=null)
            OnDoDamageCallbacks.ForEach(e => e.Invoke(snapshot.damage));
        elapsedTimeSinceLastDamage = 0f;
        return true;
    }

    public OnlineDamageSnapshot MakeSnapshot()
    {
        return new OnlineDamageSnapshot(this);
    }
}
