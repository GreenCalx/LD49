using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialRouletteArrow : MonoBehaviour
{
    public Rigidbody rb;

    void OnTriggerEnter(Collider iCollider)
    {
        rb.AddForce(Vector3.up * 10f, ForceMode.Impulse);
    }
}
