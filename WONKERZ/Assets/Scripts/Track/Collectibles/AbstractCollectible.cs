using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractCollectible : MonoBehaviour
{
    protected abstract void OnCollect();
    
    void OnTriggerEnter(Collider iCollider)
    {
        CarController cc = iCollider.GetComponent<CarController>();
        if (!!cc)
        {
            CollectiblesManager cm = Access.CollectiblesManager();
            cm.addToJar(this);
            OnCollect();
        }
    }
}
