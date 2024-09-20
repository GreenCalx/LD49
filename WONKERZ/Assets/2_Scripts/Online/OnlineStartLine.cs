using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Schnibble;
using Schnibble.Managers;
using Wonkerz;

using Mirror;

// Network object specs:
//
// Client-side:
//
// Is responsible:
//    - for player ready state
//      ! careful ! Ready state here is not the NetworkReady state !
//                  this is the state when player has loded and we are waiting to launch the track.
//                  The track is ready to be launched when every client will set its startLine ready state to true.
//    - for ready/not ready UX
//
// Server-side:
//
// Should not be used.

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
    public OnlinePlayerController OPC;

    private UIStartTrack UIStartTrackInst = null;

    public override void OnStartClient() {
        UIReadyUpHandle.SetActive(false);
        UIIsReadyHandle.SetActive(false);
        StartCoroutine(Init());
    }

    override public void OnStopClient() {
        if (OPC && OPC.self_PlayerController && OPC.self_PlayerController.inputMgr) OPC.self_PlayerController.inputMgr.Detach(this as IControllable);
    }

    void Update()
    {
        if (OPC!=null) transform.position = OPC.self_PlayerController.GetTransform().position;
    }

    IEnumerator WaitForDependencies() {
        while (OnlineGameManager.singleton             == null) {yield return null;}
        while (OnlineGameManager.singleton.localPlayer == null) {yield return null;}
    }

    IEnumerator Init()
    {
        yield return StartCoroutine(WaitForDependencies());

        var OGM = OnlineGameManager.singleton;

        OPC           = OGM.localPlayer;
        OGM.startLine = this;

        transform.position = OPC.transform.position;


        if (OGM.waitForPlayersToBeReady)
        {
            UIReadyUpHandle.SetActive(true);
            OPC.self_PlayerController.inputMgr.Attach(this as IControllable);
        }
    }

    public void LaunchCountdown()
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

    IEnumerator countdownCo()
    {
        //OPC.self_PlayerController.Freeze();
        var OGM = OnlineGameManager.singleton;
        //Play sounds each seconds.
        int lastTime = 0;
        while (OGM.countdownElapsed > 0.0f)
        {
            // check if we pass a second, from 3 to 2 for instance.
            int  currentTime   = (int)OGM.countdownElapsed;
            bool passedASecond = lastTime != currentTime;
            lastTime = currentTime;

            if (passedASecond) {
                audioSource.clip = countDownSFX0;
                audioSource.Play(0);
            }

            UIStartTrackInst.updateDisplay(OGM.countdownElapsed, passedASecond);

            yield return null;
        }
        // Play sound at the end.
        audioSource.clip = countDownSFX1;
        audioSource.Play(0);
        // Make sure we show 0
        UIStartTrackInst.updateDisplay(0);

        launchTrack();
    }

    void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
    {
        if (Entry.GetButtonState((int)PlayerInputs.InputCode.UIStart).down) {
            var localPlayer = OnlineGameManager.singleton.localPlayer;
            var readyState = !localPlayer.isReady;

            UIIsReadyHandle.SetActive(readyState);
            UIReadyUpHandle.SetActive(!readyState);

            localPlayer.CmdModifyReadyState(readyState);
        }
    }

    public void launchTrack()
    {
        if (!!startLineCrossed_SFX)
        {
            startLineCrossed_SFX.Play();
        }

        // Reset track infinite collectibles
        // Access.managers.collectiblesMgr.resetInfCollectibles();

        // start line crossed !! gogogo
        // Scene currentScene = SceneManager.GetActiveScene();
        // Access.managers.trackMgr.launchTrack(currentScene.name);

        // var states =Access.Player().vehicleStates;
        // states.SetState(states.states[(int)PlayerVehicleStates.States.Car]);

        if (enable_tricks) activateTricks();

        StartCoroutine(postCountdown());
    }


    IEnumerator postCountdown()
    {
        if (!isClient) yield break;

        meshModelHandle.gameObject.SetActive(false);

        yield return new WaitForSeconds(2f);

        Destroy(UIStartTrackInst.gameObject);

        OnlineGameManager.singleton.startLine = null;

        gameObject.SetActive(false);
    }

    public void launchCinematic()
    {
        StartCoroutine(playCinematic(this));
    }

    // NOTE:
    // Why does it takes iSL as parameter?
    // Why not use the current instance?
    IEnumerator playCinematic(OnlineStartLine iSL)
    {
        yield return new WaitForSeconds(0.2f); // track Start delay

        iSL.entryCinematicTrigger.StartCinematic();
        while (!iSL.entryCinematicTrigger.cinematicDone)
        yield return new WaitForSeconds(0.2f);
        iSL.LaunchCountdown();
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
