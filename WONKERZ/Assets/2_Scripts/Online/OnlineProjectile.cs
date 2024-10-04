using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class OnlineProjectile : NetworkBehaviour
{
    public float lifeTime = 5f;
    private float elapsedLifeTime = 0f;

    void Start()
    {
        elapsedLifeTime = 0f;
    }
    void Update()
    {
        if (elapsedLifeTime < lifeTime)
        {
            lifeTime += Time.deltaTime;
            return;
        }
        NetworkServer.Destroy(gameObject);
    }
}
