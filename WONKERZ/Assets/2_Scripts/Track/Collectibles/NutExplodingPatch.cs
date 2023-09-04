using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NutExplodingPatch : MonoBehaviour
{
    public int n_nuts = 5;
    public float forceStr = 10f;
    public float rotStr = 1f;
    public GameObject nutRef;
    public float start_delay = 0.1f;

    private bool triggered = false;

    public void triggerPatch()
    {
        if (triggered)
            return;

        triggered = true;
        StartCoroutine(explodePatch());
    }

    IEnumerator explodePatch()
    {
        yield return new WaitForSeconds(start_delay);

        for (int i=0; i < n_nuts; i++)
        {
            var rb = Instantiate(nutRef, transform.position, transform.rotation).GetComponent<Rigidbody>();

            Vector3 randDirection = Random.insideUnitSphere;
            // to hemisphere
            randDirection.y = Mathf.Abs(randDirection.y);

            rb.AddForce(randDirection * forceStr, ForceMode.Impulse);

            Vector3 torqueDir = Random.onUnitSphere;
            rb.AddTorque(torqueDir * rotStr, ForceMode.Impulse);
        }
        Destroy(gameObject);
    }
}
