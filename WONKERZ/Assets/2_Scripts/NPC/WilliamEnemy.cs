using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using Schnibble;
using Schnibble.AI;
using static UnityEngine.Debug;

public class WilliamEnemy : WkzNPC
{
    [Header("WilliamEnemy\nMAND")]
    public Animator animator;
    public PlayerDetector weakSpot;
    public GameObject deathEffect;
    public GameObject deathGhostPSRef;
    public GameObject playerSpottedEffect;
    public PlayerDetector guardDetector;
    public PlayerDetector directHitDetector;
    public List<PlayerDamager> damagers;
    public SkinnedMeshRenderer arms;
    public Material damagerMaterialRef;

    [Header("Tweaks")]
    public float max_lariat_duration = 5f;
    public float lariat_rot_per_sec = 1f;
    public float lariat_target_reached_epsilon = 1f;
    public float guard_duration = 3f;
    public float direct_hit_duration = 1.5f;
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
    public string param_DHIT = "DIRECT_HIT";
    [Header("Effects")]
    public float death_effect_size = 8f;
    public float spottedMarkerDuration = 3f; 

    // AI

    private Vector3 lariat_destination = Vector3.zero;
    private float elapsed_time_in_lariat = 0f;
    private float current_idle_time = 2f;
    private float idle_timer;

    private Coroutine showDamagerCo;

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
            facePlayer();
            idle_timer += Time.deltaTime;
            return;
        }

        if (guardDetector.playerInRange)
            if (directHitDetector.playerInRange)
                CallDirectHit();
            else
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
        Log(gameObject.name + " CallGuard");
        LaunchAction(Guard(this, coordinator.playerDetector.GetTarget().transform.position));
    }

    public void CallLariat()
    {
        Log(gameObject.name + " CallLariat");
        LaunchAction(LariatSpin(this, coordinator.playerDetector.GetTarget().transform.position));
    }

    public void CallDirectHit()
    {
        Log(gameObject.name + " CallDirectHit");
        LaunchAction(DirectHit(this, coordinator.playerDetector.GetTarget().transform.position));
    }

    private IEnumerator DirectHit(WilliamEnemy iAttacker, Vector3 iTargetPos)
    {
        iAttacker.DirectHitAnim();
        agent.isStopped = true;
        float timer = 0f;
        while (timer < guard_duration)
        {
            timer += Time.deltaTime;
            facePlayer();
            yield return null;
        }
        
        updateCurrentIdleTime();
        idle_timer = 0f;
        iAttacker.StopAction();
        iAttacker.IdleAnim();
        agent.isStopped = false;
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
                lariat_destination = GetNextLimitPosition(lariat_destination, 50f);
                if (!agent.SetDestination(lariat_destination))
                {
                    LogError("William : Failed to attack player");
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
            facePlayer();
            yield return null;
        }
        iAttacker.IdleAnim();

        updateCurrentIdleTime();
        idle_timer = 0f;
        agent.isStopped = false;
        iAttacker.StopAction();
    }

    private IEnumerator ColorizeHands(WilliamEnemy iAttacker, Color iAlbedoColor, float iDuration)
    {
        if (damagerMaterialRef==null)
            yield break;
        Material[] mats = iAttacker.arms.materials;
        Color baseColor = Color.white;
        Material damagingMat = null;
        for (int i=0;i<mats.Length;i++) 
        {
            damagingMat = mats[i];
            if (damagingMat.name==(iAttacker.damagerMaterialRef.name+Constants.EXT_INSTANCE))
            {
                baseColor = damagingMat.color;
                damagingMat.color = iAlbedoColor;
            }
        }

        if (iDuration < 0f)
            yield break;

        float timer = 0f;
        while (timer < iDuration)
        { timer += Time.deltaTime; yield return null;}


        damagingMat.color = baseColor;
    }

    public void kill()
    {
        ai_kill();
        DeathAnim();
        foreach(var damager in damagers) { Destroy(damager); }

        GameObject explosion = Instantiate(deathEffect, transform.position, Quaternion.identity);
        explosion.transform.localScale = transform.localScale * death_effect_size;
        explosion.GetComponent<ExplosionEffect>().runEffect();

        GameObject ghost = Instantiate(deathGhostPSRef, transform.position, Quaternion.identity);
        
        Destroy(gameObject, 2f);
        Destroy(cameraFocusable, 0.5f);
        Destroy(this);
    }

    public void facePlayer()
    {
        Vector3 target = Access.Player().transform.position - transform.position;
        float rotSpeed = 2f * Time.deltaTime;
        float maxRotMagDelta = 0f;

        Vector3 newDir = Vector3.RotateTowards(transform.forward, target, rotSpeed, maxRotMagDelta);
        transform.rotation = Quaternion.LookRotation(newDir);
    }


    // ANIMS
    public void GuardAnim()
    {
        animator.SetBool(param_GUARD, true);
        animator.SetBool(param_ATK, false);
        animator.SetBool(param_SURPRISED, false);
        animator.SetBool(param_TAUNT, false);
        animator.SetBool(param_DHIT, false);
        
        showDamagerCo = StartCoroutine(ColorizeHands(this, Color.red, -1));
    }

    public void IdleAnim()
    {
        animator.SetBool(param_GUARD, false);
        animator.SetBool(param_ATK, false);  
        animator.SetBool(param_SURPRISED, false);
        animator.SetBool(param_TAUNT, false);
        animator.SetBool(param_DHIT, false);

        StartCoroutine(ColorizeHands(this, Color.white, -1f));
    }

    public void LariatAnim()
    {
        animator.SetBool(param_GUARD, false);
        animator.SetBool(param_ATK, true);  
        animator.SetBool(param_SURPRISED, false);
        animator.SetBool(param_TAUNT, false);
        animator.SetBool(param_DHIT, false);

        StartCoroutine(ColorizeHands(this, Color.red, -1f));
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
        animator.SetBool(param_DHIT, false);
    }

    public void TauntAnim()
    {
        animator.SetBool(param_TAUNT, true);
        animator.SetBool(param_GUARD, false);
        animator.SetBool(param_ATK, false);  
        animator.SetBool(param_SURPRISED, false);
        animator.SetBool(param_DHIT, false);
    }

    public void DirectHitAnim()
    {
        animator.SetBool(param_TAUNT, false);
        animator.SetBool(param_GUARD, false);
        animator.SetBool(param_ATK, false);  
        animator.SetBool(param_SURPRISED, false); 

        animator.SetBool(param_DHIT, true);
    }
}
