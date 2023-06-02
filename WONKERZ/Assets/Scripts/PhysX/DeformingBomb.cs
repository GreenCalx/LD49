using UnityEngine;
using Schnibble;

public class DeformingBomb : MonoBehaviour
{
    public float explosionRange = 1f;
    public int numberOfBounceBeforeExplosion = 1;
    public GameObject explosionEffect;
    public float explosionForce = 10f;
    public int explosionDamage = 5;

    private int n_bounces;
    public float minTimeBetweenBounces = 0.3f;
    private float elapsedTimeBetweenBounces;

    public PlayerDetector playerDetector;
    public Texture2D bounceColorRamp;

    // Start is called before the first frame update
    void Start()
    {
        n_bounces = 0;
        elapsedTimeBetweenBounces = 0f;
        updateBombColor();
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTimeBetweenBounces += Time.deltaTime;
    }

    private void explode()
    {
        // GFX
        if (!!explosionEffect)
        {
            GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            explosion.GetComponent<ExplosionEffect>().runEffect();
        }

        // if player is around, do damage and push away
        if (!!playerDetector)
        {
            if (playerDetector.playerInRange || playerDetector.dummyInRange)
            {
                PlayerController player = Access.Player();
                Vector3 pushBackDir = Vector3.zero;
                pushBackDir = (player.gameObject.transform.position - transform.position);
                player.takeDamage(explosionDamage, transform.position, pushBackDir, explosionForce);
            }
        }

        // destroy the bomb
        Destroy(gameObject);
    }

    private void OnCollision(Collision other)
    {
        if (elapsedTimeBetweenBounces < minTimeBetweenBounces)
            return;

        n_bounces++;
        if (n_bounces < numberOfBounceBeforeExplosion)
        { updateBombColor(); elapsedTimeBetweenBounces = 0f; return; }

        //if (other.gameObject.GetComponent<MeshDeform>() != null)
        //{
            this.Log("Collided with a DeformMesh : " + other.gameObject.name);
            movePoints(other.gameObject, other.contacts[0].point);
            explode();
        //}
    }

    void OnCollisionEnter(Collision other)
    {
        OnCollision(other);
    }

    void OnCollisionStay(Collision other)
    {
        OnCollision(other);
    }

    private void updateBombColor()
    {
        if (bounceColorRamp==null)
            return;

        float width = bounceColorRamp.width;
        float bounce_ratio = (float)n_bounces / (float)numberOfBounceBeforeExplosion;
        
        Color newcolor = bounceColorRamp.GetPixel((int)(bounce_ratio*width),0);
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (!!mr)
        {
            mr.material.SetColor("_Color", newcolor);
        }
    }

    public void movePoints(GameObject other, Vector3 contactPoint)
    {
        MeshDeform md = other.GetComponent<MeshDeform>();
        if (md==null)
            return; // nothing to deform

        Vector3[] otherVerts = other.GetComponent<MeshDeform>().originalVertices;
        Vector3 localColPos = transform.InverseTransformPoint(contactPoint);
        this.Log("Deformed collision point at : " + contactPoint);
        float distance;
        for (int i = 0; i < otherVerts.Length; i++)
        {
            distance = Vector3.Distance(contactPoint, other.transform.TransformPoint(otherVerts[i]));
            if (distance <= explosionRange)
            {
                other.GetComponent<MeshDeform>().StartDisplacement(otherVerts[i]);
                this.Log("Deformed!!!  " + i + " : " + other.transform.TransformPoint(otherVerts[i]));
            }
        }
        //other.GetComponent<MeshDeform>().UpdateMesh(otherVerts);

    }
}
