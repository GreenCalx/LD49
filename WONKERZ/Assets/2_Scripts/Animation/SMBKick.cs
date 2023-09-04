using UnityEngine;

public class SMBKick : StateMachineBehaviour
{
    [Range(0f, 1f)]
    public float startKickAt;
    [Range(0f, 1f)]
    public float stopKickAt;
    private KickAction kick;

    private void refreshKickAction(Animator animator)
    {
        if (kick == null)
        {
            kick = animator.transform.parent.GetComponent<KickAction>();
            if (kick == null)
            {
                kick = animator.transform.parent.GetComponentInChildren<KickAction>();
            }
        }
    }
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        refreshKickAction(animator);

        // time = XX,yyy where X is number of anim loop and Y the progress of current loop 
        float time = stateInfo.normalizedTime;

        float anim_progress = time % 1;
        if ((anim_progress >= startKickAt) && (!kick.kicking))
            kick.kick();
        else if ((anim_progress >= stopKickAt) && (kick.kicking))
            kick.stopKick();
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        refreshKickAction(animator);
        kick.stopKick();
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
