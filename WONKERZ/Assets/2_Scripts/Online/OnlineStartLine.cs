using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Schnibble;
using Schnibble.Managers;

using Mirror;

public class OnlineStartLine : NetworkBehaviour, IControllable
{
    [Header("MAND")]
    //public string track_name;
    
    public GameObject UIStartTrackRef;
    public AudioSource  startLineCrossed_SFX;
    public GameObject meshModelHandle;
    public GameObject UIReadyUpHandle;
    public GameObject UIIsReadyHandle;
    public bool destroyOnActivation = true;

    public bool enable_tricks = false;
    public GameObject UIHandle; // for tricktracker

    public CinematicTrigger entryCinematicTrigger;

    public AudioSource audioSource;
    public AudioClip countDownSFX0;
    public AudioClip countDownSFX1;

    [Header("Internal")]
    public bool playerReadyUp = false;
    public OnlinePlayerController OPC;
    public OfflineGameManager OffMGR;

    private UIStartTrack UIStartTrackInst = null;


    void Start()
    {
        playerReadyUp = false;
        UIReadyUpHandle.SetActive(false);
        UIIsReadyHandle.SetActive(false);
        if (isClient)
            StartCoroutine(InitCo());
        else
            Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (isClient)
            Access.Player().inputMgr.Detach(this as IControllable);
    }
    
    IEnumerator InitCo()
    {
        OffMGR = Access.OfflineGameManager();
        while (OffMGR == null)
        {
            OffMGR = Access.OfflineGameManager();
            yield return null;
        }
        OffMGR.startLine = this;

        while(OPC==null)    
        {
            OPC = Access.Player()?.GetComponent<OnlinePlayerController>();
            yield return null;
        }
        init();

        UIReadyUpHandle.SetActive(true);
        while(!playerReadyUp)
        {
            yield return null;
        }
        UIReadyUpHandle.SetActive(false);
        UIIsReadyHandle.SetActive(true);
        if (isServer)
            NetworkRoomManagerExt.singleton.onlineGameManager.NotifyPlayerIsReady(OPC, true);
        if (isClientOnly)
            OPC.CmdNotifyOGMPlayerReady();
        // notify someone this startline is gut to go
    }



    public void init()
    {
        transform.position = OPC.transform.position;
        OPC.self_PlayerController.inputMgr.Attach(this as IControllable);
    }

    public void launchCountdown()
    {
        UIIsReadyHandle.SetActive(false);

        if (OPC==null)
            return;

        if (UIStartTrackInst==null)
        {
            UIStartTrackInst = Instantiate(UIStartTrackRef).GetComponent<UIStartTrack>();

            StartCoroutine(countdownCo());
        }
    }

    void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
    {
        if (playerReadyUp)
            return;

        var start = Entry.Get((int)PlayerInputs.InputCode.UIStart) as GameInputButton;
        if (start != null)
        {
            if (start.GetState().down)
            {
                playerReadyUp = true;
            }
        }
    }

    public void launchTrack()
    {
        if (!!startLineCrossed_SFX)
        {
            startLineCrossed_SFX.Play();
        }

        // Reset track infinite collectibles
        // Access.CollectiblesManager().resetInfCollectibles();

        // start line crossed !! gogogo
        // Scene currentScene = SceneManager.GetActiveScene();
        // Access.TrackManager().launchTrack(currentScene.name);

        // var states =Access.Player().vehicleStates;
        // states.SetState(states.states[(int)PlayerVehicleStates.States.Car]);

        if (enable_tricks)
            activateTricks();

        StartCoroutine(postCountdown());
    }

    IEnumerator countdownCo()
    {
        OPC.self_PlayerController.Freeze();

        OnlineGameManager OGM = NetworkRoomManagerExt.singleton.onlineGameManager;
        while (OGM==null)
        {
            OGM = NetworkRoomManagerExt.singleton.onlineGameManager;
            yield return null;
        }
        while(OGM.countdownElapsed > 0.1f) // not synced yet
        {
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

        OPC.self_PlayerController.UnFreeze();
    }

    IEnumerator postCountdown()
    {
        if (!isClient)
            yield break;

        // if (destroyOnActivation)
        // {
        //     Destroy(gameObject);
        // }
        meshModelHandle.gameObject.SetActive(false);

        yield return new WaitForSeconds(2f);
        
        Destroy(UIStartTrackInst.gameObject);

        OffMGR.startLine = null;
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
        TrickTracker tt = OPC.gameObject.GetComponent<TrickTracker>();
        if (!!tt)
        {
            tt.activate_tricks = true; // activate default in hub
            tt.init(UIHandle);
        }
    }
}
