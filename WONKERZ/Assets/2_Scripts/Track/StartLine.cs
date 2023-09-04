using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Schnibble;

public class StartLine : MonoBehaviour
{
    [Header("MAND")]
    //public string track_name;
    public GameObject UIStartTrackRef;
    public AudioSource  startLineCrossed_SFX;
    public bool destroyOnActivation = true;

    private float countdownElapsedTime = 0f;

    public bool enable_tricks = false;

    public float countdown_delay = 1f;
    private float countdown_delay_current = 0f;
    public bool is_rdy_to_launch = false;

    public CinematicTrigger entryCinematicTrigger;

    public AudioSource audioSource;
    public AudioClip countDownSFX0;
    public AudioClip countDownSFX1;

    [Header("Debug")]
    public bool bypassCountdown = false;

    private UIStartTrack UIStartTrackInst = null;
    // Start is called before the first frame update
    void Start()
    {
        countdownElapsedTime = 0f;
    }

    void FixedUpdate()
    {
        if (UIStartTrackInst != null)
        {
            countdownElapsedTime += Time.fixedDeltaTime;
        }
    }

    public void launchCountdown()
    {
        if (UIStartTrackInst==null)
        {
            UIStartTrackInst = Instantiate(UIStartTrackRef).GetComponent<UIStartTrack>();
            countdownElapsedTime = 0f;

            StartCoroutine(countdown(Access.Player()));
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

        if (enable_tricks)
            activateTricks();

        StartCoroutine(postCountdown());
    }

    IEnumerator countdown(PlayerController iPC)
    {
        iPC.Freeze();

        audioSource.clip = countDownSFX0;
        audioSource.Play(0);

        while (countdownElapsedTime < 3f)
        {
            if (countdownElapsedTime >= 1f && countdownElapsedTime < 1.1f) {
                audioSource.clip = countDownSFX0;
                audioSource.Play(0);
            }
            if (countdownElapsedTime >= 2f && countdownElapsedTime < 2.1f)
            {
                audioSource.clip = countDownSFX0;
                audioSource.Play(0);
            }
            
            UIStartTrackInst.updateDisplay(countdownElapsedTime);
            yield return new WaitForSeconds(0.1f);
        }

        audioSource.clip = countDownSFX1;
        audioSource.Play(0);

        UIStartTrackInst.updateDisplay(countdownElapsedTime);
        launchTrack();
        iPC.UnFreeze();
    }

    IEnumerator postCountdown()
    {
        yield return new WaitForSeconds(2f);
        
        Destroy(UIStartTrackInst.gameObject);
        if (destroyOnActivation)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerStay(Collider iCol)
    {
        if ((Utils.colliderIsPlayer(iCol)))
        {
            if (!is_rdy_to_launch)
            {
                countdown_delay_current += Time.deltaTime;
                is_rdy_to_launch = countdown_delay_current >= countdown_delay;
                Access.Player().Freeze();
            }
            else
            {
                if (bypassCountdown) // debug
                {
                    Access.CollectiblesManager().resetInfCollectibles();
                    Scene currentScene = SceneManager.GetActiveScene();
                    Access.TrackManager().launchTrack(currentScene.name);
                    if (enable_tricks)
                    activateTricks();
                    return;
                }

                if (entryCinematicTrigger!=null)
                    launchCinematic();
                else
                    launchCountdown();
                
            }
        }
    }

    public void launchCinematic()
    {
        StartCoroutine(playCinematic(this));
    }

    IEnumerator playCinematic(StartLine iSL)
    {
        yield return new WaitForSeconds(0.2f); // track Start delay

        iSL.entryCinematicTrigger.StartCinematic();
        while (!iSL.entryCinematicTrigger.cinematicDone)
            yield return new WaitForSeconds(0.2f);
        iSL.launchCountdown();
    }

    private void activateTricks()
    {
        TrickTracker tt = Access.Player().gameObject.GetComponent<TrickTracker>();
        if (!!tt)
        {
            tt.activate_tricks = true; // activate default in hub
            tt.init();
        }
    }
}
