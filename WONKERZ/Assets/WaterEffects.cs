using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WaterEffects : MonoBehaviour
{
    public GameObject underWater;

    void OnTriggerEnter(Collider c) {
        Debug.Log("OnTriggerEnter");
        underWater.SetActive(true);
    }

    void OnTriggerExit(Collider c) {
        Debug.Log("OnTriggerExit");
        underWater.SetActive(false);
    }

    void OnCollisionEnter(Collision c) {
        Debug.Log("OnCollisionEnter");
    }

    void OnCollisionExit(Collision c) {
        Debug.Log("OnCollisionExit");
    }
}
