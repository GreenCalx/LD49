using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionCallbacks : MonoBehaviour
{
    public UnityEvent OnCollisionEnterCallback;
    public bool playerOnly = true;

    void OnCollisionEnter(Collision iCollision)
    {
        if (playerOnly)
        {
            if (!Utils.collisionIsPlayer(iCollision))
            {
                return;
            }
        }

        if (OnCollisionEnterCallback!=null)
            OnCollisionEnterCallback.Invoke();
    }

}
