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

    [Header("Tweaks")]
    public string navAgentMaskName = "Walkable";
    public float lariat_rot_per_sec = 1f;
    public float guard_duration = 3f;
    public float lariat_target_reached_epsilon = 1f;
    public float idle_time_between_action = 2f;
    public float idle_time_variance = 0.5f;
    [Header("Anim")]
    public string param_ATK = "ATTACK";
    public string param_GUARD = "GUARD";

    // AI
    private bool in_action = false;
    private bool playerAggroed = false;
    private bool previously_attacked = false;
    private Vector3 lariat_destination = Vector3.zero;

    private float idle_timer;
    private NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent> ();
        idle_timer = 0f;
        playerAggroed = false;
        in_action = false;
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

    private void OnAggro()
    {
        if (lariat_destination!=Vector3.zero)
            return;
        StartCoroutine(LariatSpin(this, playerDetector.player.position));
        //transform.LookAt(playerDetector.player);
    }

    private void InAggro()
    {
        if (lariat_destination!=Vector3.zero)
            return;

        if (in_action)
            return;

        if (idle_timer < idle_time_between_action)
            return;

        if (!previously_attacked)
            StartCoroutine(LariatSpin(this, playerDetector.player.position));
        else
            StartCoroutine(Guard(this, playerDetector.player.position));
    }

    private IEnumerator LariatSpin(WilliamEnemy iAttacker, Vector3 iTarget)
    {
        weakSpot.enabled = false;
        
        iAttacker.in_action = true;
        lariat_destination = iTarget;
        if (!agent.SetDestination(lariat_destination))
        {
            Debug.LogError("William : Failed to attack player");
        }
        animator.SetBool(param_ATK, true);

        float rotSpeed = 360f / lariat_rot_per_sec;
        while( Vector3.Distance(iAttacker.transform.position, iTarget) > lariat_target_reached_epsilon)
        {
            iAttacker.transform.Rotate(0, rotSpeed * Time.deltaTime, 0, Space.Self);
            Debug.Log("Remainin distance : " + Vector3.Distance(iAttacker.transform.position, iTarget));
            yield return null;
        }

        animator.SetBool(param_ATK, false);
        lariat_destination = Vector3.zero;
        previously_attacked = true;
        iAttacker.in_action = false;
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
        idle_timer = 0f;
    }

    public void kill()
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }
}
