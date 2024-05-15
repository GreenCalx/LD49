using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("Bomb")]
    public float explosionRange = 1f;
    public float explosionForce = 10f;
    public int explosionDamage = 5;
    public AudioSource boomSFX;
    public Transform explosionEffectOrigin;
    public GameObject explosionEffect;
    public bool checkIfPlayerIsInSight = true;
    private bool exploded = false;

    public void explode()
    {
        if (exploded)
            return;

        // GFX
        if (explosionEffect!=null)
        {
            GameObject explosion = Instantiate(explosionEffect, explosionEffectOrigin.position, Quaternion.identity);
            explosion.transform.localScale = new Vector3(explosionRange, explosionRange, explosionRange);
            explosion.GetComponent<ExplosionEffect>().runEffect();
        }

        // SFX
        if (boomSFX!=null)
            Schnibble.Utils.SpawnAudioSource(boomSFX, transform);

        // if player is around, do damage and push away
        PlayerController player = Access.Player();

        if (Vector3.Distance(player.transform.position, explosionEffectOrigin.position) <= explosionRange)
        { // player in distance
            if (checkIfPlayerIsInSight)
            {
                Vector3 vecToPlayer = player.transform.position - transform.position;
                RaycastHit hitInfo;
                bool playerUndercover = Physics.SphereCast( transform.position, 1f, vecToPlayer, out hitInfo, explosionRange);
                if (!playerUndercover)
                {
                    if (Utils.colliderIsPlayer(hitInfo.collider))
                    {
                        Vector3 pushBackDir = Vector3.zero;
                        pushBackDir = (player.gameObject.transform.position - transform.position);
                        player.takeDamage(explosionDamage, transform.position, pushBackDir, explosionForce);
                    }
                }
            } else {
                Vector3 pushBackDir = Vector3.zero;
                pushBackDir = (player.gameObject.transform.position - transform.position);
                player.takeDamage(explosionDamage, transform.position, pushBackDir, explosionForce);
            }
        }



        // destroy the bomb
        exploded = true;
        Destroy(gameObject);
    }


}