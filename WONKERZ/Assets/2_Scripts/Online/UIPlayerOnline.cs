using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

using Schnibble;
using Schnibble.UI;

using Wonkerz;

public class UIPlayerOnline : UIElement 
{
    public float nutTurnAnimDuration = 0.25f;
    public float nutAnimAngle = 90;
    public Transform OverDriveUIHandle;
    public Transform UINutImg;
    public TMPro.TextMeshProUGUI    speedText;
    public Image                    speedBar;
    public Image                    throttleBar;
    public Image                    lifeBar;
    public TextMeshProUGUI          lifePool;
    public TextMeshProUGUI          driveGearLbl;
    public TextMeshProUGUI          reverseGearLbl;
    //public TextMeshProUGUI          equippedPower;
    [Header("Powers")]
    public Transform equippedPowerHandle;
    public Image equippedPowerThumbnailImage;
    public Image powerCooldownBar;
    public Transform        DitchPowerHintHandle;
    public Color weaponInCooldownColor = Color.green;
    public Color weaponArmedColor = Color.red;
    public Color weaponInRecoveryColor = Color.yellow;
    [Header("TrackEvents")]
    public Transform        TrackEventHandle;
    public Transform        TrackEventOnFXHandle;
    public Transform        TrackEventOffFXHandle;
    public TextMeshProUGUI  TrackEventNameTxt;
    public UIElement ItsTrialTimeHandle;
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

    protected override void Start()
    {
        if (OverDriveUIHandle!=null)
            OverDriveUIHandle.gameObject.SetActive(false);
        nutAnimMutex = false;

        StartCoroutine(Schnibble.Utils.CoroutineChain(
            WaitForGameManager(),
            WaitForLocalPlayer()));
    }

    void ShowUITrackTime(bool value) {
        showTrackTime = value;
    }

    IEnumerator WaitForGameManager() {
        while (OnlineGameManager.singleton == null) {
            yield return null;
        }
        OnlineGameManager.singleton.onShowUITrackTime += ShowUITrackTime;
    }

    IEnumerator WaitForLocalPlayer() {

        while (OnlineGameManager.singleton.localPlayer == null) {
            yield return null;
        }

        LinkToPlayer(OnlineGameManager.singleton.localPlayer);
    }

    public void LinkToPlayer(OnlinePlayerController iOPC)
    {
        onlinePlayer = iOPC;
        player = iOPC.self_PlayerController;

        updateSpeedCounter();
        updateThrottleCounter();
        updateLifePool(true);
        updateGearShift();
    }

    protected override void Update()
    {
        if (player==null)
        { return;}

        updateSpeedCounter();
        updateThrottleCounter();
        updateLifePool(false);
        updateTrackTime();
        updateGearShift();
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
        if (OverDriveUIHandle!=null)
            OverDriveUIHandle.gameObject.SetActive( (speed > maxSpeed) );
    }

    public void updateThrottleCounter()
    {
        if (player==null) return;

        float maxTorque = (float) player.car.car.motor.mutDef.maxTorque;
        float currTorque = (float) player.car.car.motor.GetOutput( player.car.car.GetThrottle() );

        throttleBar.fillAmount = currTorque / maxTorque;
    }

    public void updateLifePool(bool iForce)
    {
        int n_nuts = onlinePlayer.bag.nuts;

        if ((n_nuts!=currNutsInBank) || iForce)
        {
            lifePool.text = n_nuts.ToString();

            rotateNutCo = StartCoroutine(RotateUINut(nutTurnAnimDuration, n_nuts > currNutsInBank));

            WkzCar wCar = onlinePlayer.self_PlayerController.car.car as WkzCar;
            WkzCarSO as_wkzso = wCar.mutDef as WkzCarSO;
            lifeBar.fillAmount = ((float) n_nuts) /  ((float)as_wkzso.nutCapacity);

            currNutsInBank = n_nuts;
        }
    }

    public void updateGearShift()
    {
        SchCar asSchCar = onlinePlayer.self_PlayerController.car.car;
        bool isInDriveMode = !asSchCar.gearBox.invertInput;
        
        driveGearLbl.gameObject.SetActive(isInDriveMode); 
        reverseGearLbl.gameObject.SetActive(!isInDriveMode);
    }

    public void updateAvailablePanels(int iVal)
    {
        int val = (iVal > 0) ? iVal : 0 ;
        
        string str = (val < 999) ? val.ToString() : "inf";

        nAvailablePanels.text = str;
    }

    public void updateCPFillImage(float iVal)
    {
        cpImageFilled.fillAmount = iVal;
    }
    public void updateCarPowerUI(ICarPower iPower)
    {
        if (iPower.isArmed)
            powerCooldownBar.color = weaponArmedColor;
        else if (iPower.isRecovering)
            powerCooldownBar.color = weaponInRecoveryColor;
        else
            powerCooldownBar.color = weaponInCooldownColor;

        if (iPower.isRecovering)
        {
            var ratio = Mathf.Clamp01( iPower.elapsed_recovery_cooldown / iPower.recovery_cooldown);
            powerCooldownBar.fillAmount = 1f - ratio;
        } else
        {
            // in cooldown
            var ratio = Mathf.Clamp01(iPower.elapsed_cooldown / iPower.cooldown);
            powerCooldownBar.fillAmount = ratio;
        }
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
