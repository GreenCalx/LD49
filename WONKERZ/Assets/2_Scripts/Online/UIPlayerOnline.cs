using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Schnibble;
using TMPro;

public class UIPlayerOnline : MonoBehaviour
{
    public float nutTurnAnimDuration = 0.25f;
    public float nutAnimAngle = 90;
    public Transform OverDriveUIHandle;
    public Transform UINutImg;
    public TMPro.TextMeshProUGUI    speedText;
    public Image                    speedBar;
    public TextMeshProUGUI          lifePool;

    public Image            cpImageFilled;
    public TextMeshProUGUI  nAvailablePanels;
    public TextMeshProUGUI idOfLastCPTriggered;

    public bool showTrackTime = false;
    public TextMeshProUGUI onlineTrackTime;

    [Header("Internals")]
    public OnlinePlayerController onlinePlayer;
    private PlayerController player;


    private int currNutsInBank = 0;
    private Coroutine rotateNutCo;
    private bool nutAnimMutex;
    void Start()
    {
        updateLastCPTriggered("x");
        cpImageFilled.fillAmount = 0f;

        OverDriveUIHandle.gameObject.SetActive(false);
        nutAnimMutex = false;

        // updateSpeedCounter();
        // updateLifePool();
    }

    public void LinkToPlayer(OnlinePlayerController iOPC)
    {
        onlinePlayer = iOPC;
        player = iOPC.self_PlayerController;

        updateSpeedCounter();
        updateLifePool();
    }

    void Update()
    {
        if (player==null)
        { return;}

        updateSpeedCounter();
        updateLifePool();
        updateTrackTime();
    }

    private void updateTrackTime()
    {
        if (!showTrackTime)
        {
            if (onlineTrackTime.gameObject.activeSelf)
            {
                onlineTrackTime.gameObject.SetActive(false);
            }
            return;
        }

        if (!onlineTrackTime.gameObject.activeSelf)
        {
            onlineTrackTime.gameObject.SetActive(true);
        }

        float trackTime = NetworkRoomManagerExt.singleton.onlineGameManager.gameTime;
        int trackTime_val_min = (int)(trackTime / 60);
        if (trackTime_val_min<0)
        {
            trackTime_val_min = 0;
        }
        string trackTime_str_min = trackTime_val_min.ToString();
        if (trackTime_str_min.Length<=1)
        {
            trackTime_str_min = "0"+trackTime_str_min;
        }

        int trackTime_val_sec = (int)(trackTime % 60);
        if (trackTime_val_sec<0)
        {
            trackTime_val_min = 0;
        }
        string trackTime_str_sec = trackTime_val_sec.ToString();
        if (trackTime_str_sec.Length<=1)
        {
            trackTime_str_sec = "0"+trackTime_str_sec;
        }

        onlineTrackTime.text = trackTime_str_min +":"+ trackTime_str_sec;
        
    }

    public void updateSpeedCounter()
    {
        if (player==null)
        {
            return;
        }
        var PlayerVelocity = player.rb.velocity.magnitude;
        CarController cc = player.car;
        float max_speed = cc.maxTorque;

        // Update Bar
        float bar_percent = Mathf.Clamp(PlayerVelocity / max_speed, 0f, max_speed);
        speedBar.fillAmount = bar_percent;

        // Update Text
        string lbl = ((int)PlayerVelocity).ToString();
        speedText.SetText(lbl);

        //update overdrive
        OverDriveUIHandle.gameObject.SetActive( (PlayerVelocity > max_speed) );
    }

    public void updateLifePool()
    {
        int n_nuts = onlinePlayer.bag.nuts;

        lifePool.text = (n_nuts > 0) ? n_nuts.ToString() : "0";

        if (n_nuts > currNutsInBank)
        {
            // Rotate ui nut CW
            rotateNutCo = StartCoroutine(RotateUINut(nutTurnAnimDuration, true));

        } else if ( n_nuts < currNutsInBank)
        {
            // Rotate ui nut ccw
            rotateNutCo = StartCoroutine(RotateUINut(nutTurnAnimDuration, false));
        }

        currNutsInBank = n_nuts;
    }

    public void updateAvailablePanels(int iVal)
    {
        int val = (iVal > 0) ? iVal : 0 ;
        
        string str = (val < 999) ? val.ToString() : "inf";

        nAvailablePanels.text = str;
    }

    public void updateLastCPTriggered(string iTxt)
    {
        idOfLastCPTriggered.text = iTxt;
    }

    public void updateCPFillImage(float iVal)
    {
        cpImageFilled.fillAmount = iVal;
    }
    public void updatePanelOnRespawn()
    {

    }

    IEnumerator RotateUINut(float iAnimDuration, bool RotateCW)
    {

        while (nutAnimMutex)
        { yield return null; }

        nutAnimMutex = true;

        float elapsedTime = 0;
        Quaternion initRot = UINutImg.rotation;
        Quaternion targetRot = Quaternion.identity;
        if (RotateCW)
            targetRot = Quaternion.Euler(UINutImg.rotation.eulerAngles + new Vector3(0,0,nutAnimAngle));
        else
            targetRot = Quaternion.Euler(UINutImg.rotation.eulerAngles + new Vector3(0,0,-1*nutAnimAngle));
        while (elapsedTime < iAnimDuration)
        {
            UINutImg.rotation = Quaternion.Lerp(initRot, targetRot, elapsedTime/iAnimDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        UINutImg.rotation = targetRot;

        nutAnimMutex = false;
    }
}