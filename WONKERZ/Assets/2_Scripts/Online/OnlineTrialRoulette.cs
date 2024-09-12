using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void init(List<string> iTrials)
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

        // Place in middle of cam viewport
        CameraManager CM = Access.CameraManager();
        Camera cam = CM.active_camera.cam;
        
        transform.parent = cam.transform;
        Vector3 point = Vector3.zero;
        transform.localPosition = Vector3.zero;

        transform.localPosition += new Vector3(0f, 0f, fwd_offset);

        transform.localRotation = Quaternion.identity;
        roulette_rb.maxAngularVelocity = 60f;
    }    

    // Call me within FixedUpdate or im FUCKED UP
    public void Spin()
    {
        roulette_rb.isKinematic = false; 
        roulette_rb.AddRelativeTorque( roulette_rb.transform.right * spinForce, ForceMode.Impulse);
    }

    public void StopSpin()
    {
        // 8 faces cylinder, we want a face to show str8 towards cam
        Vector3 angles = roulette_rb.transform.rotation.eulerAngles;
        float snapAngle = angles.x % 45f;

        Debug.Log("Snap^Angle :" + snapAngle);

        roulette_rb.isKinematic = true;
        roulette_rb.angularVelocity = Vector3.zero;


        roulette_rb.transform.Rotate(snapAngle, 0f,0f);
    }

    public bool IsSpinning()
    {
        return roulette_rb.angularVelocity.magnitude > Vector3.one.magnitude;
    }

    public string RetrieveSelectedTrial()
    {
        float simpAngle = roulette_rb.transform.rotation.eulerAngles.x % 360;
        int selectedFace = (int)(simpAngle / 45f);
        TextMeshProUGUI selectedTxt = trialLabelHandles[selectedFace];

        selectedTxt.overrideColorTags = true;
        selectedTxt.color = Color.yellow;
        selectedTxt.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        selectedTxt.text = "selected";

        return selectedTxt.text;
    }

}
