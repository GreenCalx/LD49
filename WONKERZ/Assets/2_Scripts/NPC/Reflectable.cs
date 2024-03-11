using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///   Reflectable projectile by SpinAttack
/// </summary>
public class Reflectable : MonoBehaviour
{
    [Header("Tweaks")]
    public float reflectionMultiplier = 1f;
    [Header("Optionals")]
    public Transform autoAimOnReflection;
    [Header("Internals")]
    public bool isReflected = false;

    public bool tryReflect()
    {
        if (isReflected)
            return false;
        
        PlayerDamager pdamager = GetComponent<PlayerDamager>();
        if (!!pdamager)
        {
            Destroy(pdamager);
        }

        EnemyDamager edamager = GetComponent<EnemyDamager>();
        if (edamager==null)
        {
            gameObject.AddComponent<EnemyDamager>();
        }

        isReflected = true;
        return true;
    }

}
