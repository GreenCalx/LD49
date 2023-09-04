using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMBGasStationActivation : StateMachineBehaviour
{
    public ParticleSystem[] PSOnActivation;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PSOnActivation = animator.transform.parent.GetComponentsInChildren<ParticleSystem>(true);
        if (PSOnActivation!=null)
        {
            for (int i=0; i<PSOnActivation.Length; i++)
            {
                PSOnActivation[i].gameObject.SetActive(true);
                PSOnActivation[i].Play();
            }
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (PSOnActivation!=null)
        {
            for (int i=0; i<PSOnActivation.Length; i++)
            {
                PSOnActivation[i].Stop();
            }
        }
    }
}
