using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class OnlineCollectiblePatch : NetworkBehaviour
{
    // Start is called before the first frame update
    public int spawn_count = 5;
    [Range(0f,1f)]
    public float chanceForStatToBePositive = 0.6f; 
    public float forceStr = 10f;
    public float rotStr = 1f;
    public float upwardMultiplier = 1f;
    public List<OnlineCollectible> collectiblePrefabs;
    public float start_delay = 0.1f;
    public bool triggerOnStart = false;
    private bool triggered = false;

    void Start()
    {
        if (!isServer)
            return;
        
        if (triggerOnStart)
            triggerPatch();
    }
    
    [Server]
    public void triggerPatch()
    {
        if (triggered)
            return;

        triggered = true;
        StartCoroutine(explodePatch());
    }

    IEnumerator explodePatch()
    {
        if (collectiblePrefabs.Count==0)
            yield break;

        yield return new WaitForSeconds(start_delay);

        for (int i=0; i < spawn_count; i++)
        {
            // Random Range with int has an exclusive max value
            GameObject chosenCollectible = collectiblePrefabs[Random.Range(0,collectiblePrefabs.Count)].gameObject;
            bool IsNegative = Random.Range(0.0f,1.0f) < chanceForStatToBePositive;
            
            GameObject spawnedCollect = Instantiate(chosenCollectible, transform.position, transform.rotation);
            spawnedCollect.transform.parent = null;

            if (IsNegative)
            {
                OnlineCollectible asCollectible = spawnedCollect.GetComponent<OnlineCollectible>();
                if (asCollectible.collectibleType != ONLINE_COLLECTIBLES.NUTS)
                    asCollectible.SetAsNegative();
            }

            NetworkServer.Spawn(spawnedCollect);

            var rb = spawnedCollect.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;

            Vector3 randDirection = Random.insideUnitSphere;
            spawnedCollect.transform.position -= randDirection;
            // to hemisphere
            randDirection.y = Mathf.Abs(randDirection.y) * upwardMultiplier;

            rb.AddForce(randDirection * forceStr, ForceMode.Impulse);

            Vector3 torqueDir = Random.onUnitSphere;
            rb.AddTorque(torqueDir * rotStr, ForceMode.Impulse);
        }
        Destroy(gameObject);
    }
}
