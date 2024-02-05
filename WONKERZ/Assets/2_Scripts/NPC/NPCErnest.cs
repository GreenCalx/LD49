using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCErnest : NPCDialog
{
    [Header("# NPCErnest")]
    [Header("Self References")]
    public Animator selfAnimator;
    [Header("Anim States")]

    public string anim_HAPPY = "";
    public string anim_THINKER = "";
    public string anim_POINTHAND = "";
    public string anim_SAD = "";
    [Header("Anim Actions")]
    public string anim_QuitAction = "";
    public string anim_DoAction = "";
    public string anim_RotateCW = "";
    public string anim_RotateCCW = "";
    public string anim_EUREKA = "";
    public string anim_VICTORY = "";
    public string anim_POINTFINGER = "";

    // -

    private Coroutine actRotateCo;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //FacePlayer();// temp test
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
    public void BeSad(bool iState)
    {
        selfAnimator.SetBool(anim_SAD, iState);
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

    public void ActEureka()
    {
        selfAnimator.SetTrigger(anim_EUREKA);
        selfAnimator.SetTrigger(anim_DoAction);
    }
    public void ActRotateCW()
    {
        selfAnimator.SetTrigger(anim_RotateCW);
        selfAnimator.SetTrigger(anim_DoAction);

        if (actRotateCo!=null)
        { StopCoroutine(actRotateCo); actRotateCo = null; }
        actRotateCo = StartCoroutine(FacePlayer());
    }
    public void ActRotateCCW()
    {
        selfAnimator.SetTrigger(anim_RotateCCW);
        selfAnimator.SetTrigger(anim_DoAction);

        if (actRotateCo!=null)
        { StopCoroutine(actRotateCo); actRotateCo = null; }
        actRotateCo = StartCoroutine(FacePlayer());
    }

    public void ActRotateTowardsPlayer()
    {
        PlayerController p_ref = Access.Player();
        Vector3 pos = p_ref.transform.position;
        pos.y = transform.position.y;
        Quaternion targetRot = Quaternion.LookRotation(transform.position - pos);

        Vector3 vecSelfRot = transform.rotation * Vector3.up;
        Vector3 vecTargetRot = targetRot * Vector3.up;

        float angSelfRot    = Mathf.Atan2(vecSelfRot.x, vecSelfRot.z) * Mathf.Rad2Deg;
        float angTargetRot  = Mathf.Atan2(vecTargetRot.x, vecTargetRot.z) * Mathf.Rad2Deg;
        var angleDiff = Mathf.DeltaAngle( angTargetRot, angSelfRot);
        if (angleDiff > 0 )
        {
            ActRotateCW();
        } else {
            ActRotateCCW();
        }
    }

    // Behaviours
    IEnumerator FacePlayer()
    {
        PlayerController p_ref = Access.Player();
        float timeCount = 0f;
        float animSpeed = 0.04f;

            Vector3 pos = p_ref.transform.position;
            pos.y = transform.position.y;

            Quaternion targetRot = Quaternion.LookRotation(transform.position - pos);

        while (timeCount < selfAnimator.speed)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, timeCount * animSpeed);

            timeCount += Time.deltaTime;
            yield return null;
        }
        selfAnimator.SetTrigger(anim_QuitAction);
        
    }
}
