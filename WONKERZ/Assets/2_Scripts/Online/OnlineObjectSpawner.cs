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
    public bool centerSpawnedItem = false;
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
    [Header("Internals")]
    public bool isCoordinated = false;
    private float elapsedTimeSinceLastSpawn;
    public List<GameObject> spawnedObjects;

    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Color gizmoColor = Color.yellow;
        gizmoColor.a = 0.5f;
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(transform.position, transform.lossyScale);
    }
    void OnDrawGizmos()
    {
        Color gizmoColor = Color.yellow;
        gizmoColor.a = 0.2f;
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(transform.position, transform.lossyScale);
    }
    #endif

    // Start is called before the first frame update
    void Start()
    {
        elapsedTimeSinceLastSpawn = 0f;
        spawnedObjects = new List<GameObject>();
        
        xmin = transform.position.x + boundaries.bounds.min.x * transform.lossyScale.x;
        xmax = transform.position.x + boundaries.bounds.max.x * transform.lossyScale.x;

        ymin = transform.position.y + boundaries.bounds.min.y * transform.lossyScale.y;
        ymax = transform.position.y + boundaries.bounds.max.y * transform.lossyScale.y;

        zmin = transform.position.z + boundaries.bounds.min.z * transform.lossyScale.z;
        zmax = transform.position.z + boundaries.bounds.max.z * transform.lossyScale.z;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer)
            return;

        if (!NetworkRoomManagerExt.singleton.onlineGameManager.gameLaunched)
            return;

        elapsedTimeSinceLastSpawn += Time.deltaTime;

        if (isCoordinated)
            return;

        PollForSpawn();
    }

    void OnDestroy()
    {
        if (isCoordinated)
            return;

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
    public float Evaluate()
    {
        return spawnProbabilityOverTime.Evaluate(elapsedTimeSinceLastSpawn * spawnProbabilityTimeScale);
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
        if (IsFull())
        { return; }

        Vector3 spawn_loc = Vector3.zero;
        if (centerSpawnedItem)
            spawn_loc = new Vector3(    transform.position.x, 
                                        transform.position.y, 
                                        transform.position.z);
        else
            spawn_loc = new Vector3(    Random.Range(xmin,xmax), 
                                        Random.Range(ymin,ymax), 
                                        Random.Range(zmin,zmax));
        
        
        Vector3 planeNormal = new Vector3(0,-1,0);
        
        GameObject SelectedPrefab = ObjectsToSpawn[Random.Range(0,ObjectsToSpawn.Count)];

        GameObject NewObject = Instantiate(SelectedPrefab);

        NewObject.transform.parent = null;
        NewObject.transform.position = spawn_loc;
        
        NetworkServer.Spawn(NewObject);

        spawnedObjects.Add(NewObject);
    }

    [Server]
    public bool IsFull()
    {
        return spawnedObjects.Count >= MAX_SPAWN ;
    }
}
