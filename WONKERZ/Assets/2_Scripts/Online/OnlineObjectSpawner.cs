using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using Mirror;

/**
*   TODO : Object Pooling
*/
public class OnlineObjectSpawner : NetworkBehaviour
{
    // Tweaks
    public List<GameObject> ObjectsToSpawn;
    public int MAX_SPAWN = 15;
    public AnimationCurve spawnProbabilityOverTime;
    public float spawnProbabilityTimeScale;

    // Boundaries
    public Mesh boundaries;
    private float xmin;
    private float xmax;
    private float ymin;
    private float ymax;
    private float zmin;
    private float zmax;

    // internals
    private float elapsedTimeSinceLastSpawn;
    private List<GameObject> spawnedObjects;

    // Start is called before the first frame update
    void Start()
    {
        elapsedTimeSinceLastSpawn = 0f;
        spawnedObjects = new List<GameObject>();
        
        xmin = boundaries.bounds.min.x * transform.lossyScale.x;
        xmax = boundaries.bounds.max.x * transform.lossyScale.x;

        ymin = boundaries.bounds.min.y * transform.lossyScale.y;
        ymax = boundaries.bounds.max.y * transform.lossyScale.y;

        zmin = boundaries.bounds.min.z * transform.lossyScale.z;
        zmax = boundaries.bounds.max.z * transform.lossyScale.z;
    }

    // Update is called once per frame
    void Update()
    {
        if (!NetworkRoomManagerExt.singleton.onlineGameManager.gameLaunched)
            return;

        if (isServer)
        {
            elapsedTimeSinceLastSpawn += Time.deltaTime;
            PollForSpawn();
        }
    }

    void OnDestroy()
    {
        if (isServer)
            UnspawnAll();
    }

    [Server]
    public void PollForSpawn()
    {
        float prob = spawnProbabilityOverTime.Evaluate(elapsedTimeSinceLastSpawn * spawnProbabilityTimeScale);
        float rand_res = Random.Range(0f, 1f);
        if (rand_res<prob)
        {
            SpawnObject();
            elapsedTimeSinceLastSpawn = 0f;
        }
    }

    [Server]
    public void UnspawnAll()
    {
        foreach(GameObject go in spawnedObjects)
        {
            NetworkServer.Destroy(go);
        }
    }

    [Server]
    public void SpawnObject()
    {
        if (ObjectsToSpawn.Count<=0)
        { return; }

        spawnedObjects = spawnedObjects.Where(e => e != null).ToList();
        if (spawnedObjects.Count>=MAX_SPAWN)
        { return; }

        Vector3 spawn_loc = new Vector3(Random.Range(xmin,xmax), Random.Range(ymin,ymax), Random.Range(zmin,zmax));
        Vector3 planeNormal = new Vector3(0,-1,0);
        
        GameObject SelectedPrefab = ObjectsToSpawn[Random.Range(0,ObjectsToSpawn.Count)];

        GameObject NewObject = Instantiate(SelectedPrefab);
        NewObject.transform.position = spawn_loc;
        NewObject.transform.parent = null;

        NetworkServer.Spawn(NewObject);

        spawnedObjects.Add(NewObject);
    }
}
