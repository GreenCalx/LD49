using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using TMPro;
using Mirror;
using Wonkerz;
/**
*   To be displayed in center of cam viewport + fwd offset for each player
*   > thus only sync its rotation
*   
*/
public class OnlineTrialRoulette : NetworkBehaviour
{

    public readonly float fwd_offset = 10f;
    public float spinForce = 900000000f;
    public List<TextMeshProUGUI> trialLabelHandles;
    public Rigidbody roulette_rb;
    public float snapAngleSpeed = 10f;
    public bool HasSnapped = false;

    public void init(List<string> iTrials)
    {
        RpcInitRouletteLabels(iTrials);
        // Place in middle of cam viewport
        RpcRelocateRoulette();
        roulette_rb.maxAngularVelocity = 60f;
    }

    [ClientRpc]
    public void RpcInitRouletteLabels(List<string> iTrials)
    {
        int n_trials = iTrials.Count;
        if (n_trials < 1)
        {
            Debug.LogError("No trials available");
        }

        // init trial labels
        for (int i=0; i<trialLabelHandles.Count; i++)
        {
            if (n_trials==1)
            {
                trialLabelHandles[i].text  = iTrials[0];
            }
            if (i>=n_trials)
            {
                trialLabelHandles[i].text = iTrials[i%n_trials];
                continue;
            }
            trialLabelHandles[i].text = iTrials[i];
        }
    }

    [ClientRpc]
    public void RpcRelocateRoulette()
    {
        CameraManager CM = Access.managers.cameraMgr;
        Camera cam = CM.active_camera.cam;
        
        transform.parent = cam.transform;
        Vector3 point = Vector3.zero;
        transform.localPosition = Vector3.zero;

        transform.localPosition += new Vector3(0f, 0f, fwd_offset);

        transform.localRotation = Quaternion.identity;
    }

    // Call me within FixedUpdate or im FUCKED UP
    public void Spin()
    {
        roulette_rb.isKinematic = false; 
        roulette_rb.AddRelativeTorque( roulette_rb.transform.right * spinForce, ForceMode.Impulse);
    }

    public void StopSpin()
    {
        roulette_rb.angularVelocity = Vector3.zero;
        roulette_rb.isKinematic = true;
        HasSnapped = true;
        //StartCoroutine(SnapAngleCo());
    }

    // IEnumerator SnapAngleCo()
    // {
    //     HasSnapped = false;
    //     Quaternion initRot = transform.rotation;

    //     Vector3 angles = roulette_rb.transform.rotation.eulerAngles;
    //     float snapAngle = angles.x + (angles.x % 45f);
    //     Quaternion targetRot = Quaternion.Euler(new Vector3(snapAngle, angles.y, angles.z));

    //     float timeCount = 0f;
    //     while (Quaternion.Angle(transform.rotation, targetRot) > 0f)
    //     {
    //         transform.rotation = Quaternion.Lerp(initRot, targetRot, timeCount * snapAngleSpeed );
    //         timeCount += Time.deltaTime;
    //         yield return null;
    //     }
    //     HasSnapped = true;
    // }

    public bool IsSpinning()
    {
        return roulette_rb.angularVelocity.magnitude > 0.05f;
    }

    public string RetrieveSelectedTrial()
    {
        float simpAngle = roulette_rb.transform.rotation.eulerAngles.x % 360f;
        int selectedFace = (int)Mathf.Floor(simpAngle / 45f);
        TextMeshProUGUI selectedTxt = trialLabelHandles[selectedFace];

        ShowSelectedTrial(selectedFace);
        if (isServer)
            RpcShowSelectedTrial(selectedFace);

        return selectedTxt.text;
    }

    [ClientRpc]
    public void RpcShowSelectedTrial(int iSelectedFace)
    {
        ShowSelectedTrial(iSelectedFace);
    }

    public void ShowSelectedTrial(int iSelectedFace)
    {
        TextMeshProUGUI selectedTxt = trialLabelHandles[iSelectedFace];
        selectedTxt.overrideColorTags = true;
        selectedTxt.color = Color.yellow;
        selectedTxt.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

}
