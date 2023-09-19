using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Schnibble;

public class WilliamEnemy : SchAIAgent
{
    [Header("MAND")]
    public Animator animator;
    public PlayerDetector weakSpot;
    public GameObject deathEffect;
    public GameObject playerSpottedEffect;

    [Header("Tweaks")]
    public float lariat_rot_per_sec = 1f;
    public float guard_duration = 3f;
    public float lariat_target_reached_epsilon = 1f;
    public float idle_time_between_action = 2f;
    public float idle_time_variance = 0.5f;
    [Range(0f,50f)]
    public float player_aim_error = 5f;
    [Header("Anim")]
    public string param_ATK = "ATTACK";
    public string param_GUARD = "GUARD";
    public float death_effect_size = 8f;
    public float max_lariat_duration = 5f;
    public float spottedMarkerDuration = 3f; 
    // AI

    private Vector3 lariat_destination = Vector3.zero;
    private float elapsed_time_in_lariat = 0f;
    private float current_idle_time = 2f;
    private float idle_timer;

    // Start is called before the first frame update
    void Start()
    {
        ai_init();
        idle_timer = 0f;
        playerSpottedEffect.SetActive(false);
        updateCurrentIdleTime();
    }

    // Update is called once per frame
    void Update()
    {
        if (weakSpot.playerInRange && !in_action)
        {
            kill();
        }
    }

    public void updateCurrentIdleTime()
    {
        current_idle_time = idle_time_between_action + Random.Range(idle_time_variance*-1, idle_time_variance);
    }

    protected override void OnAggro()
    {
        if (lariat_destination!=Vector3.zero)
            return;
        StartCoroutine(ShowSpottedMarker(this));
    }

    protected override void InAggro()
    {
        if (lariat_destination!=Vector3.zero)
            return;

        if (idle_timer < current_idle_time)
        {
            idle_timer += Time.deltaTime;
            return;
        }
        LaunchAction(LariatSpin(this, coordinator.playerDetector.player.position));   
    }

    protected override void OutAggro()
    {}

    public void CallGuard()
    {
        if (in_action)
            return;
        if (actionCo != null)
            StopCoroutine(actionCo);
        LaunchAction(Guard(this, coordinator.playerDetector.player.position));
    }

    private IEnumerator ShowSpottedMarker(WilliamEnemy iSelf)
    {
        playerSpottedEffect.SetActive(true);
        float show_elapsed = 0f;
        float anim_freq = 1 / iSelf.spottedMarkerDuration;

        while (show_elapsed < iSelf.spottedMarkerDuration)
        {
            show_elapsed += Time.deltaTime;
            yield return null;
        }

        playerSpottedEffect.SetActive(false);
    }

    // private IEnumerator LariatSpin(WilliamEnemy iAttacker, Vector3 iTarget)
    // {
    //     weakSpot.enabled = false;
    //     elapsed_time_in_lariat = 0f;

    //     lariat_destination = iTarget;
    //     Vector3 error = Random.insideUnitCircle * player_aim_error;
    //     lariat_destination.x += error.x * Random.Range(-1f,1f);
    //     lariat_destination.z += error.z * Random.Range(-1f,1f);;

    //     if (!agent.SetDestination(lariat_destination))
    //     {
    //         Debug.LogError("William : Failed to attack player");
    //     }
    //     animator.SetBool(param_ATK, true);

    //     float rotSpeed = 360f / lariat_rot_per_sec;
    //     while( Vector3.Distance(iAttacker.transform.position, lariat_destination) > lariat_target_reached_epsilon)
    //     {
    //         iAttacker.transform.Rotate(0, rotSpeed * Time.deltaTime, 0, Space.Self);
    //         elapsed_time_in_lariat += Time.deltaTime;
    //         if (elapsed_time_in_lariat >= max_lariat_duration)
    //             break;
            
    //         yield return null;
    //     }

    //     animator.SetBool(param_ATK, false);
    //     lariat_destination = Vector3.zero;
    //     iAttacker.in_action = false;

    //     updateCurrentIdleTime();
    //     idle_timer = 0f;

    //     weakSpot.enabled = true;
    // }

    private IEnumerator LariatSpin(WilliamEnemy iAttacker, Vector3 iTarget)
    {
        weakSpot.enabled = false;
        elapsed_time_in_lariat = 0f;

        animator.SetBool(param_ATK, true);

        float rotSpeed = 360f / lariat_rot_per_sec;
        while( Vector3.Distance(iAttacker.transform.position, lariat_destination) > lariat_target_reached_epsilon)
        {
            lariat_destination = GetNextPositionFromCoordinator();

            if (!agent.SetDestination(lariat_destination))
            {
                // WHY ?
                // Find if its because the destination is out of the navigated surface
                // > If so, find the limit position on this surface aligned with computed prediction
                // > Else, try to move randomly somewhere on the surface
                lariat_destination = GetNextLimitPosition(lariat_destination);
                if (!agent.SetDestination(lariat_destination))
                {
                    Debug.LogError("William : Failed to attack player");
                }
            }
            iAttacker.transform.Rotate(0, rotSpeed * Time.deltaTime, 0, Space.Self);

            // Limit time in lariat
            elapsed_time_in_lariat += Time.deltaTime;
            if (elapsed_time_in_lariat >= max_lariat_duration)
                break;
            
            yield return null;
        }

        agent.ResetPath();
        animator.SetBool(param_ATK, false);
        lariat_destination = Vector3.zero;
        iAttacker.in_action = false;

        updateCurrentIdleTime();
        idle_timer = 0f;

        weakSpot.enabled = true;
    }

    private IEnumerator Guard(WilliamEnemy iAttacker, Vector3 iTarget)
    {
        float timer = 0f;
        animator.SetBool(param_GUARD, true);
        agent.ResetPath();
        while (timer < guard_duration)
        {
            timer += Time.deltaTime;
            iAttacker.transform.LookAt(iTarget);
            yield return null;
        }
        animator.SetBool(param_GUARD, false);
        iAttacker.in_action = false;

        updateCurrentIdleTime();
        idle_timer = 0f;
    }

    public void kill()
    {
        GameObject explosion = Instantiate(deathEffect, transform.position, Quaternion.identity);
        explosion.transform.localScale = transform.localScale * death_effect_size;
        explosion.GetComponent<ExplosionEffect>().runEffect();
        
        ai_kill();
        
        Destroy(gameObject);
    }
}
