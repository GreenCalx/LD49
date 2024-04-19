using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingMortar : MonoBehaviour
{
    [Header("Tweaks")]
    public float timeBeforeFallingOnTarget = 2f;
    public GameObject explosionEffect;
    [Header("Internals")]
    public bool isFalling = false;
    public Vector3 positionTarget;
    private float elapsedTime;
    private Rigidbody rb;
    private SphereCollider sc;
    // Start is called before the first frame update
    void Start()
    {
        elapsedTime = 0f;
        isFalling = false;
        rb = GetComponent<Rigidbody>();
        sc = GetComponent<SphereCollider>();
        sc.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isFalling)
        {
            if (elapsedTime < timeBeforeFallingOnTarget)
            {
                elapsedTime += Time.deltaTime;
                return;
            }
            rb.velocity = Vector3.zero;
            sc.enabled = true;
            isFalling = true;
        }
        transform.position = new Vector3(positionTarget.x, transform.position.y, positionTarget.z);
    }

    void OnCollisionEnter(Collision iCol)
    {
        explode();
    }

    public void explode()
    {
        // GFX
        if (!!explosionEffect)
        {
            GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            explosion.transform.localScale = transform.localScale * 1.2f;
            explosion.GetComponent<ExplosionEffect>().runEffect();
        }

        Destroy(gameObject);
    }
}
