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
    private float minTimeBetweenBounces = 0.1f;
    private float elapsedTimeBetweenBounces;

    public PlayerDetector playerDetector;

    // Start is called before the first frame update
    void Start()
    {
        n_bounces = 0;
        elapsedTimeBetweenBounces = 999f;
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
                CarController cc = Access.Player();
                Vector3 pushBackDir = Vector3.zero;
                pushBackDir = (cc.gameObject.transform.position - transform.position);
                cc.takeDamage(explosionDamage, transform.position, pushBackDir, explosionForce);
            }
        }

        // destroy the bomb
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision other)
    {

        if (elapsedTimeBetweenBounces < minTimeBetweenBounces)
            return;

        n_bounces++;
        if (n_bounces < numberOfBounceBeforeExplosion)
            return;

        if (other.gameObject.GetComponent<MeshDeform>() != null)
        {
            this.Log("Collided with a DeformMesh : " + other.gameObject.name);
            movePoints(other.gameObject, other.contacts[0].point);
            explode();
        }
    }

    public void movePoints(GameObject other, Vector3 contactPoint)
    {
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
