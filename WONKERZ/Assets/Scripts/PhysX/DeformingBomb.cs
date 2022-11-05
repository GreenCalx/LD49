using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeformingBomb : MonoBehaviour
{
    public float explosionRange = 1f;
    public int numberOfBounceBeforeExplosion=1;
    public GameObject explosionEffect;

    private int n_bounces;
    private float minTimeBetweenBounces = 0.1f;
    private float elapsedTimeBetweenBounces;
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
        if (!!explosionEffect)
        {
            GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            explosion.GetComponent<ExplosionEffect>().runEffect();
        }
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision other) {
         
        if (elapsedTimeBetweenBounces < minTimeBetweenBounces)
            return;

        n_bounces++;
        if (n_bounces < numberOfBounceBeforeExplosion)
            return;

         if (other.gameObject.GetComponent<MeshDeform>() != null) {
            Debug.Log("Collided with a DeformMesh : " + other.gameObject.name);
             movePoints(other.gameObject, other.contacts[0].point);
             explode();
         }
     }

    public void movePoints(GameObject other, Vector3 contactPoint) {
        Vector3[] otherVerts = other.GetComponent<MeshDeform>().originalVertices;
        Vector3 localColPos = transform.InverseTransformPoint(contactPoint);
        Debug.Log("Deformed collision point at : " + contactPoint);
         float distance;
         for (int i=0; i<otherVerts.Length; i++) {
             distance = Vector3.Distance(contactPoint, other.transform.TransformPoint(otherVerts[i]));
             if (distance <= explosionRange) {
                  other.GetComponent<MeshDeform>().StartDisplacement(otherVerts[i]);
                  Debug.Log("Deformed!!!  "+ i + " : " + other.transform.TransformPoint(otherVerts[i]));
             }
         }
        //other.GetComponent<MeshDeform>().UpdateMesh(otherVerts);
        
     }
}
