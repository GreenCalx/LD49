using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrabClaw : MonoBehaviour
{
    public Transform UpClaw;
    public float ClosedClawRotOffset = 10f;
    public float TimeToCloseClaw = 0.5f;
    public float CloseClawDelay = 1f;

    public float WiggleAngle = 5f;
    public float WiggleFrequency = 5f;

    private Quaternion BaseRot;
    private Quaternion UpClawBaseRot;
    private float WiggleElapsed;
    private bool GoLeft = false;

    private bool animMutex = false;

    // Start is called before the first frame update
    void Start()
    {
        BaseRot = transform.rotation;
        UpClawBaseRot = UpClaw.localRotation;
        WiggleElapsed = 0f;
        animMutex = false;
    }

    // Update is called once per frame
    void Update()
    {
        WiggleElapsed += Time.deltaTime;
        if (WiggleElapsed > WiggleFrequency)
        {
            if (!GoLeft)
                transform.rotation = Quaternion.Euler( BaseRot.eulerAngles + new Vector3(WiggleAngle,0f,0f));
            else
                transform.rotation = Quaternion.Euler( BaseRot.eulerAngles + new Vector3(-1*WiggleAngle,0f,0f));
            GoLeft = !GoLeft;
            WiggleElapsed = 0f;
        }
    }

    public void CloseClaw()
    {
        if (animMutex)
            return;

        StartCoroutine(DelayedCloseClawCo());
    }

    IEnumerator DelayedCloseClawCo()
    {
        animMutex = true;

        float delay = 0f;
        while (delay < CloseClawDelay)
        { delay += Time.deltaTime; yield return null;}

        // Close claw
        float animElapsed = 0f;
        Quaternion initRot = UpClawBaseRot;
        Quaternion targetRot = Quaternion.Euler(UpClaw.localRotation.eulerAngles + new Vector3(ClosedClawRotOffset,0,0));
        while (animElapsed < TimeToCloseClaw)
        {
            UpClaw.localRotation = Quaternion.Lerp(initRot, targetRot, animElapsed/TimeToCloseClaw);
            animElapsed += Time.deltaTime;
            yield return null;
        }
        UpClaw.localRotation = targetRot;

        // open claw
        initRot = UpClaw.localRotation;
        targetRot = UpClawBaseRot;
        animElapsed = 0f;
        while (animElapsed < TimeToCloseClaw)
        {
            UpClaw.localRotation = Quaternion.Lerp(initRot, targetRot, animElapsed/TimeToCloseClaw);
            animElapsed += Time.deltaTime;
            yield return null;
        }
        UpClaw.localRotation = targetRot;

        animMutex = false;
    }
}
