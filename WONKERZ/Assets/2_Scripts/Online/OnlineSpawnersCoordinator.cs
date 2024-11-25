using System.Collections;
using System.Collections.Generic;

using System.Linq;
using Mirror;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[ExecuteInEditMode]
#endif
public class OnlineSpawnersCoordinator : NetworkBehaviour
{
    public List<OnlineObjectSpawner> spawners;
    public int MAX_TOT_SPAWNS = 50;
    public int TOT_SPAWNED = 0;
    public float PollingTimeStep = 1f;

    #if UNITY_EDITOR
    public bool autoFindChildSpawners = false;
    void Update()
    {
        if (Application.isPlaying)
            return;
        if (autoFindChildSpawners)
        {
            spawners = new List<OnlineObjectSpawner>(GetComponentsInChildren<OnlineObjectSpawner>());
            foreach (var s in spawners)
            {
                NetworkIdentity NI = s.gameObject.GetComponent<NetworkIdentity>();
                if (NI!=null)
                {
                    DestroyImmediate(NI);
                    EditorUtility.SetDirty(s);
                }
            }
            EditorUtility.SetDirty(this);
            autoFindChildSpawners = false;
        }
    }
    #endif

    // Start is called before the first frame update
    void Start()
    {
        #if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
        #endif
        foreach(var spawner in spawners)
        { spawner.isCoordinated = true; }
        TOT_SPAWNED = 0;
        StartCoroutine(CoordinateCo());
    }

    IEnumerator CoordinateCo()
    {
        while (NetworkRoomManagerExt.singleton.onlineGameManager.gameLaunched)
        {
            PollForSpawns();
            yield return new WaitForSeconds(PollingTimeStep);
        }

        foreach(var s in spawners) { s.UnspawnAll(); }
    }

    public void PollForSpawns()
    {
        // Evaluate all spawners
        // Ignore all spawners that are already full
        Dictionary<OnlineObjectSpawner, float> evaluatedSpawners = new Dictionary<OnlineObjectSpawner, float>();
        TOT_SPAWNED = 0;
        foreach( var s in spawners)
        { 
            TOT_SPAWNED += s.spawnedObjects.Count;

            if (s.IsFull())
                continue;
            evaluatedSpawners.Add(s, s.Evaluate()); 
        }

        // Exit if limit already exceeded
        if (TOT_SPAWNED >= MAX_TOT_SPAWNS)
            return;

        // sort by proba
        var orderedEvaluatedSpawners = evaluatedSpawners.OrderBy(e => e.Value).ToDictionary(k => k.Key, e => e.Value);

        // try to spawn in order until full
        foreach( var s in orderedEvaluatedSpawners.Keys)
        {
            float rand_res = Random.Range(0f, 1f);
            if (rand_res <= orderedEvaluatedSpawners[s])
            { 
                s.SpawnObject(); 
                TOT_SPAWNED++;
            }
            if (TOT_SPAWNED >= MAX_TOT_SPAWNS)
                break;
        }
    }

}
