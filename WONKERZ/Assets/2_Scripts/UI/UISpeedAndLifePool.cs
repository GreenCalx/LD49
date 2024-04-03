using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Schnibble;
using TMPro;

public class UISpeedAndLifePool : MonoBehaviour
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

    private PlayerController player;
    private CollectiblesManager CM;

    private int currNutsInBank = 0;
    private Coroutine rotateNutCo;
    private bool nutAnimMutex;
    void Start()
    {
        player = Access.Player();
        CM = Access.CollectiblesManager();

        updateLastCPTriggered("x");
        cpImageFilled.fillAmount = 0f;

        OverDriveUIHandle.gameObject.SetActive(false);
        nutAnimMutex = false;

        updateSpeedCounter();
        updateLifePool();
    }

    void Update()
    {
        updateSpeedCounter();
        updateLifePool();
    }

    public void updateSpeedCounter()
    {
        #if false
        PlayerController pc = Access.Player();
        float playerVelocity = 0.0f;
        float max_speed = 0.0f;
        if (pc.car_old) {
            CarController cc = pc.car_old;
            playerVelocity = cc.GetCurrentSpeed();
            max_speed = cc.maxTorque;
        } else {
            SchCarController cc = pc.car_new;
            playerVelocity = cc.GetCurrentSpeed();
            max_speed = cc.maxTorque;
        }

        // Update Bar
        float bar_percent = Mathf.Clamp(playerVelocity / max_speed, 0f, max_speed);
        speedBar.fillAmount = bar_percent;

        // Update Text
        float velocityInKmH = playerVelocity * 3.6f;
        string lbl = ((int)velocityInKmH).ToString();
        lbl += " KMH";
        speedText.SetText(lbl);

        //update overdrive
        OverDriveUIHandle.gameObject.SetActive( (PlayerVelocity > max_speed) );
        #endif
    }

    public void updateLifePool()
    {
        int n_nuts = CM.getCollectedNuts();
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
