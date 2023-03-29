using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NutExplodingPatch : MonoBehaviour
{
    public int n_nuts = 5;
    public float forceStr = 10f;
    public GameObject nutRef;
    
    private bool triggered = false;

    public void triggerPatch()
    {
        if (triggered)
            return;

        for (int i=0; i < n_nuts; i++)
        {
            GameObject nut = Instantiate(nutRef);

            nut.transform.position = transform.position;

            Rigidbody rb = nut.GetComponent<Rigidbody>();
            Vector3 randDirection = Random.insideUnitCircle;
            rb.AddForce(randDirection*forceStr, ForceMode.Impulse);
        }
        
        triggered = true;
        Destroy(gameObject);
    }
}
