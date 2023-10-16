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
    public GameObject deathGhostPSRef;
    public GameObject playerSpottedEffect;
    public PlayerDetector guardDetector;
    public List<PlayerDamager> damagers;

    [Header("Tweaks")]
    public float max_lariat_duration = 5f;
    public float lariat_rot_per_sec = 1f;
    public float lariat_target_reached_epsilon = 1f;
    public float guard_duration = 3f;
    public float idle_time_between_action = 2f;
    public float idle_time_variance = 0.5f;
    [Range(0f,50f)]
    public float player_aim_error = 5f;
    [Header("Anim")]
    public string param_ATK = "ATTACK";
    public string param_GUARD = "GUARD";
    public string param_DEATH = "DEATH";
    public string param_TAUNT = "TAUNT";
    public string param_SURPRISED = "SURPRISED";
    public float death_effect_size = 8f;

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

    /// SCHAIAgent overrides
    protected override void OnAggro()
    {
        if (lariat_destination!=Vector3.zero)
            return;
        StartCoroutine(ShowSpottedMarker(this));
        SurprisedAnim();
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

        if (guardDetector.playerInRange)
            CallGuard();
        else
            CallLariat();
    }

    protected override void OutAggro()
    {}

    protected override void PreLaunchAction()
    {
        // Prevent double launch of the same action ? 
        // > send warning as its weird.

    }

    protected override void PreStopAction()
    {
        
    }

    /// LOCAL
    public void CallGuard()
    {
        Debug.Log(gameObject.name + " CallGuard");
        LaunchAction(Guard(this, coordinator.playerDetector.player.position));
    }

    public void CallLariat()
    {
        Debug.Log(gameObject.name + " CallLariat");
        LaunchAction(LariatSpin(this, coordinator.playerDetector.player.position));
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

    private IEnumerator LariatSpin(WilliamEnemy iAttacker, Vector3 iTarget)
    {
        weakSpot.enabled = false;
        elapsed_time_in_lariat = 0f;

        iAttacker.LariatAnim();
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
        iAttacker.IdleAnim();

        lariat_destination = Vector3.zero;
        updateCurrentIdleTime();
        idle_timer = 0f;

        weakSpot.enabled = true;

        iAttacker.StopAction();
    }

    private IEnumerator Guard(WilliamEnemy iAttacker, Vector3 iTarget)
    {
        float timer = 0f;
        agent.isStopped = true;
        iAttacker.GuardAnim();
        while (timer < guard_duration)
        {
            timer += Time.deltaTime;
            //iAttacker.transform.LookAt(iTarget);
            yield return null;
        }
        iAttacker.IdleAnim();

        updateCurrentIdleTime();
        idle_timer = 0f;
        agent.isStopped = false;
        iAttacker.StopAction();
    }

    public void kill()
    {
        
        DeathAnim();
        foreach(var damager in damagers) { Destroy(damager); }

        ai_kill();

        GameObject explosion = Instantiate(deathEffect, transform.position, Quaternion.identity);
        explosion.transform.localScale = transform.localScale * death_effect_size;
        explosion.GetComponent<ExplosionEffect>().runEffect();

        GameObject ghost = Instantiate(deathGhostPSRef, transform.position, Quaternion.identity);
        
        Destroy(gameObject, 1f);
        Destroy(this);
    }


    // ANIMS
    public void GuardAnim()
    {
        animator.SetBool(param_GUARD, true);
        animator.SetBool(param_ATK, false);
        animator.SetBool(param_SURPRISED, false);
        animator.SetBool(param_TAUNT, false);
    }

    public void IdleAnim()
    {
        animator.SetBool(param_GUARD, false);
        animator.SetBool(param_ATK, false);  
        animator.SetBool(param_SURPRISED, false);
        animator.SetBool(param_TAUNT, false);
    }

    public void LariatAnim()
    {
        animator.SetBool(param_GUARD, false);
        animator.SetBool(param_ATK, true);  
        animator.SetBool(param_SURPRISED, false);
        animator.SetBool(param_TAUNT, false);
    }

    public void DeathAnim()
    {
        animator.SetBool(param_DEATH, true);  
    }

    public void SurprisedAnim()
    {
        animator.SetBool(param_SURPRISED, true);
        animator.SetBool(param_GUARD, false);
        animator.SetBool(param_ATK, false);  
        animator.SetBool(param_TAUNT, false);
    }

    public void TauntAnim()
    {
        animator.SetBool(param_TAUNT, true);
        animator.SetBool(param_GUARD, false);
        animator.SetBool(param_ATK, false);  
        animator.SetBool(param_SURPRISED, false);
    }
}
