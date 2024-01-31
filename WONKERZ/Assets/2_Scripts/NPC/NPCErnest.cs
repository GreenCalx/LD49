using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCErnest : PNJDialog
{
    [Header("NPCErnest")]
    [Header("Self References")]
    public Animator selfAnimator;
    public string anim_DoAction = "";
    public string anim_HAPPY = "";
    public string anim_THINKER = "";
    public string anim_VICTORY = "";
    public string anim_POINTFINGER = "";
    public string anim_POINTHAND = "";

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void BeHappy(bool iState)
    {
        selfAnimator.SetBool(anim_HAPPY, iState);
    }
    public void BeThinking(bool iState)
    {
        selfAnimator.SetBool(anim_THINKER, iState);
    }
    public void BePointingHand(bool iState)
    {
        selfAnimator.SetBool(anim_POINTHAND, iState);
    }

    // ACTIONS
    // Note : Order is important for AnimatorGraph. 
    // First set the target action, then DoAction to effectively launch transition towards ActionAnim
    public void ActVictory()
    {
        selfAnimator.SetTrigger(anim_VICTORY);
        selfAnimator.SetTrigger (anim_DoAction);
    }

    public void ActPointFinger()
    {
        
        selfAnimator.SetTrigger(anim_POINTFINGER);
        selfAnimator.SetTrigger (anim_DoAction);
    }
}
