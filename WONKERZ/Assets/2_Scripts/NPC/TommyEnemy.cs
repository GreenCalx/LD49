using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using Schnibble;
using Schnibble.AI;
using Schnibble.Rendering;
using static UnityEngine.Debug;

public class TommyEnemy : WkzEnemy
{
    [Header("# TommyEnemy\nMAND")]
    
    public List<GameObject> throwables;
    public Transform ballSpawnOrigin;
    public List<Transform> ballSpawningPoints;
    public NPCMouthAnimDecal faceDecalAnim;
    [Header("Tweaks")]
    
    public float throwForce = 10f;
    public float throwableDuration = 3f;
    public float throwableAtkWindUpDuration = 1f; // in sec
    [Header("Anim")]
    public float placeholder3 = 0f;
    [Header("Internals")]
    private float current_idle_time = 2f;
    private float idle_timer;
    void Start()
    {
        ai_init();
        showStaticAtkDecals(false);
    }

    void Update()
    {

    }

    protected override void OnAggro()
    {
        StartCoroutine(ShowSpottedMarker(this));
    }

    protected override void InAggro()
    {
        if (idle_timer < current_idle_time)
        {
            facePlayer();
            idle_timer += Time.deltaTime;
            return;
        }

        CallThrowBalls();
    }
    protected override void OutAggro()
    {
        // nothing?
    }

    protected override void PreLaunchAction()
    {
        // nothing?
    }
    protected override void PreStopAction()
    {
        // nothing?
    }

    public override void kill()
    {
        facePlayer(true);
        ai_kill();

        Destroy(gameObject);
    }

    // Unique methods
    public void CallThrowBalls()
    {
        Log(gameObject.name + " CallThrowBalls");
        LaunchAction(ThrowBallsCo(this));
    }

    private IEnumerator ThrowBallsCo(TommyEnemy iSelf)
    {
        if ((throwables==null)||(throwables.Count<=0))
        {
            Log(gameObject.name + " No throwables to throw");
            yield break;
        }

        if (!!faceDecalAnim)
        {
            faceDecalAnim.Animate(true);
        }

        // prepare atk
        showStaticAtkDecals(true);
        float wind_up = 0f;
        while (wind_up < throwableAtkWindUpDuration)
        {
            wind_up += Time.deltaTime;
            yield return null;
        }
        showStaticAtkDecals(false);

        // Launch ballzz
        foreach(Transform t in ballSpawningPoints)
        {
            Vector3 ballThrowDir = t.position - ballSpawnOrigin.position;
            
            int rand_ball_selec = Random.Range(0, throwables.Count);
            GameObject spawnedBall = Instantiate(throwables[rand_ball_selec]);

            spawnedBall.transform.parent = null;
            spawnedBall.transform.position = t.position;

            Rigidbody rb = spawnedBall.GetComponentInChildren<Rigidbody>();
            rb.AddForce( ballThrowDir.normalized * throwForce, ForceMode.Impulse);

            Destroy(spawnedBall.gameObject, throwableDuration);
        }

        iSelf.StopAction();
        idle_timer = 0f;

        if (!!faceDecalAnim)
        {
            faceDecalAnim.Animate(false);
        }
    }
}