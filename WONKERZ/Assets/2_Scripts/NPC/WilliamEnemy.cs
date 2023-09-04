using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Schnibble;

public class WilliamEnemy : MonoBehaviour
{
    [Header("MAND")]
    public Animator animator;
    public PlayerDetector playerDetector;
    public PlayerDetector weakSpot;
    public GameObject deathEffect;

    [Header("Tweaks")]
    public string navAgentMaskName = "Walkable";
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
    // AI
    private bool in_action = false;
    private bool playerAggroed = false;
    private bool previously_attacked = false;
    private Vector3 lariat_destination = Vector3.zero;
    private float elapsed_time_in_lariat = 0f;
    private float current_idle_time = 2f;

    private float idle_timer;
    private NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent> ();
        idle_timer = 0f;
        playerAggroed = false;
        in_action = false;
        updateCurrentIdleTime();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerDetector.playerInRange)
        {
            if (!playerAggroed)
            {
                // first attack, fresh aggro
                playerAggroed = true;
                OnAggro();
            } else {
                idle_timer += Time.deltaTime;
                // Already aggroed the player
                InAggro();
            }


        } else {
            playerAggroed = false;
            previously_attacked = false;
        }

        if (weakSpot.playerInRange && !in_action)
        {
            kill();
        }

    }

    public void updateCurrentIdleTime()
    {
        current_idle_time = idle_time_between_action + Random.Range(idle_time_variance*-1, idle_time_variance);
    }

    private void OnAggro()
    {
        if (lariat_destination!=Vector3.zero)
            return;
        StartCoroutine(LariatSpin(this, playerDetector.player.position));
    }

    private void InAggro()
    {
        if (lariat_destination!=Vector3.zero)
            return;

        if (in_action)
            return;

        if (idle_timer < current_idle_time)
        {
            //transform.LookAt(playerDetector.player);
            return;
        }

        if (!previously_attacked)
            StartCoroutine(LariatSpin(this, playerDetector.player.position));
        else
            StartCoroutine(Guard(this, playerDetector.player.position));
    }

    private IEnumerator LariatSpin(WilliamEnemy iAttacker, Vector3 iTarget)
    {
        weakSpot.enabled = false;
        elapsed_time_in_lariat = 0f;

        iAttacker.in_action = true;
        lariat_destination = iTarget;
        Vector3 error = Random.insideUnitCircle * player_aim_error;
        lariat_destination.x += error.x * Random.Range(-1f,1f);
        lariat_destination.z += error.z * Random.Range(-1f,1f);;

        if (!agent.SetDestination(lariat_destination))
        {
            Debug.LogError("William : Failed to attack player");
        }
        animator.SetBool(param_ATK, true);

        float rotSpeed = 360f / lariat_rot_per_sec;
        while( Vector3.Distance(iAttacker.transform.position, lariat_destination) > lariat_target_reached_epsilon)
        {
            iAttacker.transform.Rotate(0, rotSpeed * Time.deltaTime, 0, Space.Self);
            elapsed_time_in_lariat += Time.deltaTime;
            if (elapsed_time_in_lariat >= max_lariat_duration)
                break;
            
            yield return null;
        }

        animator.SetBool(param_ATK, false);
        lariat_destination = Vector3.zero;
        previously_attacked = true;
        iAttacker.in_action = false;

        updateCurrentIdleTime();
        idle_timer = 0f;

        weakSpot.enabled = true;
    }

    private IEnumerator Guard(WilliamEnemy iAttacker, Vector3 iTarget)
    {
        iAttacker.in_action = true;
        float timer = 0f;
        animator.SetBool(param_GUARD, true);
        while (timer < guard_duration)
        {
            timer += Time.deltaTime;
            iAttacker.transform.LookAt(iTarget);
            yield return null;
        }
        animator.SetBool(param_GUARD, false);
        previously_attacked = false;
        iAttacker.in_action = false;

        updateCurrentIdleTime();
        idle_timer = 0f;
    }

    public void kill()
    {
        StopAllCoroutines();

        GameObject explosion = Instantiate(deathEffect, transform.position, Quaternion.identity);
        explosion.transform.localScale = transform.localScale * death_effect_size;
        explosion.GetComponent<ExplosionEffect>().runEffect();

        Destroy(gameObject);
    }
}
