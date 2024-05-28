using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Schnibble;

using Mirror;

public class OnlineStartLine : NetworkBehaviour
{
    [Header("MAND")]
    //public string track_name;
    public GameObject UIStartTrackRef;
    public AudioSource  startLineCrossed_SFX;
    public GameObject meshModelHandle;
    public bool destroyOnActivation = true;

    public bool enable_tricks = false;
    public GameObject UIHandle; // for tricktracker

    public bool is_rdy_to_launch = false;

    public CinematicTrigger entryCinematicTrigger;

    public AudioSource audioSource;
    public AudioClip countDownSFX0;
    public AudioClip countDownSFX1;

    [Header("Internal")]
    private PlayerController PC;

    private UIStartTrack UIStartTrackInst = null;


    void Start()
    {
        StartCoroutine(SignUpToOfflineMgr());
    }
    
    IEnumerator SignUpToOfflineMgr()
    {
        OfflineGameManager OGM = Access.OfflineGameManager();
        while (OGM==null)
        {
            OGM = Access.OfflineGameManager();
            yield return null;
        }
        OGM.startLine = this;        
    }

    public void init(PlayerController iPC)
    {
        PC = iPC;
        transform.position = PC.transform.position;
    }

    public void launchCountdown()
    {

        if (PC==null)
            return;

        if (UIStartTrackInst==null)
        {
            UIStartTrackInst = Instantiate(UIStartTrackRef).GetComponent<UIStartTrack>();

            StartCoroutine(countdownCo(PC));
        }
    }

    public void launchTrack()
    {
        if (!!startLineCrossed_SFX)
        {
            startLineCrossed_SFX.Play();
        }

        // Reset track infinite collectibles
        Access.CollectiblesManager().resetInfCollectibles();

        // start line crossed !! gogogo
        Scene currentScene = SceneManager.GetActiveScene();
        Access.TrackManager().launchTrack(currentScene.name);

        var states =Access.Player().vehicleStates;
        states.SetState(states.states[(int)PlayerVehicleStates.States.Car]);

        if (enable_tricks)
            activateTricks();

        StartCoroutine(postCountdown());
    }

    IEnumerator countdownCo(PlayerController iPC)
    {
        iPC.Freeze();

        OnlineGameManager OGM = NetworkRoomManagerExt.singleton.onlineGameManager;
        while (OGM==null)
        {
            OGM = NetworkRoomManagerExt.singleton.onlineGameManager;
            yield return null;
        }

        audioSource.clip = countDownSFX0;
        audioSource.Play(0);
        int lastSec = 0;
        while (OGM.countdownElapsed < OGM.countdown)
        {
            if (OGM.countdownElapsed >= (lastSec+1))
            {
                audioSource.clip = countDownSFX0;
                audioSource.Play(0);
                UIStartTrackInst.updateDisplay(OGM.countdownElapsed);
                lastSec+=1;
            }     
            
            yield return null;
        }

        audioSource.clip = countDownSFX1;
        audioSource.Play(0);

        UIStartTrackInst.updateDisplay(OGM.countdownElapsed);
        launchTrack();
        iPC.UnFreeze();
    }

    IEnumerator postCountdown()
    {
        // if (destroyOnActivation)
        // {
        //     Destroy(gameObject);
        // }
        meshModelHandle.gameObject.SetActive(false);

        yield return new WaitForSeconds(2f);
        
        Destroy(UIStartTrackInst.gameObject);
        gameObject.SetActive(false);

    }

    public void launchCinematic()
    {
        StartCoroutine(playCinematic(this));
    }

    IEnumerator playCinematic(OnlineStartLine iSL)
    {
        yield return new WaitForSeconds(0.2f); // track Start delay

        iSL.entryCinematicTrigger.StartCinematic();
        while (!iSL.entryCinematicTrigger.cinematicDone)
            yield return new WaitForSeconds(0.2f);
        iSL.launchCountdown();
    }

    private void activateTricks()
    {
        TrickTracker tt = PC.gameObject.GetComponent<TrickTracker>();
        if (!!tt)
        {
            tt.activate_tricks = true; // activate default in hub
            tt.init(UIHandle);
        }
    }
}
