using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NutExplodingPatch : MonoBehaviour
{
    public int n_nuts = 5;
    public float forceStr = 10f;
    public GameObject nutRef;
    public float start_delay = 0.1f;
    private float elapsedTime = 0f;
    
    private bool triggered = false;

    public void triggerPatch()
    {
        if (triggered)
            return;
        
        elapsedTime = 0f;
        StartCoroutine(explodePatch());
        triggered = true;
    }

    IEnumerator explodePatch()
    {
        
        while (elapsedTime <= start_delay)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        for (int i=0; i < n_nuts; i++)
        {
            GameObject nut = Instantiate(nutRef);

            nut.transform.position = transform.position;

            Rigidbody rb = nut.GetComponent<Rigidbody>();
            Vector3 randDirection = Random.insideUnitCircle.normalized;
            randDirection.y *= randDirection.y;
            rb.AddForce(randDirection*forceStr, ForceMode.Impulse);
        }
        Destroy(gameObject);
    }
}
