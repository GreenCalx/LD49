using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractCollectible : MonoBehaviour
{
    public enum COLLECTIBLE_TYPE { INFINITE=0, UNIQUE=1 }
    public COLLECTIBLE_TYPE collectibleType = COLLECTIBLE_TYPE.INFINITE;

    protected abstract void OnCollect();
    
    void OnTriggerEnter(Collider iCollider)
    {
        CarController cc = iCollider.GetComponent<CarController>();
        if (!!cc)
        {
            OnCollect();
        }
    }
}
