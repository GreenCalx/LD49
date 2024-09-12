using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Schnibble;
using TMPro;
using Wonkerz;

public class UIPlayerOnline : MonoBehaviour
{
    public float nutTurnAnimDuration = 0.25f;
    public float nutAnimAngle = 90;
    public Transform OverDriveUIHandle;
    public Transform UINutImg;
    public TMPro.TextMeshProUGUI    speedText;
    public Image                    speedBar;
    public TextMeshProUGUI          lifePool;
    public TextMeshProUGUI          equippedPower;
    public Transform        DitchPowerHintHandle;
    public Transform        TrackEventHandle;
    public Transform        TrackEventOnFXHandle;
    public Transform        TrackEventOffFXHandle;
    public TextMeshProUGUI  TrackEventNameTxt;
    [Header("oldies")]
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
        StartCoroutine(Schnibble.Utils.CoroutineChain(
            WaitForGameManager(),
            WaitForLocalPlayer()));
    }

    void ShowUITrackTime(bool value) {
        showTrackTime = value;
    }

    IEnumerator WaitForGameManager() {
        while (OnlineGameManager.Get() == null) {
            yield return null;
        }
        OnlineGameManager.Get().onShowUITrackTime += ShowUITrackTime;
    }

    IEnumerator WaitForLocalPlayer() {

        while (OnlineGameManager.Get().localPlayer == null) {
            yield return null;
        }

        LinkToPlayer(OnlineGameManager.Get().localPlayer);
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

        var speed = (float)player.car.car.GetCurrentSpeedInKmH_FromWheels();
        // TODO: compute max theoretical speed from car specs.
        var maxSpeed = 300.00f;
        var ratio = Mathf.Clamp01(Mathf.Abs(speed) / maxSpeed);
        speedBar.fillAmount = ratio;
        speedText.SetText(((int)speed).ToString());

        //update overdrive
        OverDriveUIHandle.gameObject.SetActive( (speed > maxSpeed) );
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
