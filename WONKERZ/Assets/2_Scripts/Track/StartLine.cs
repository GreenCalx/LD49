using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Schnibble;

namespace Wonkerz
{

    public class StartLine : MonoBehaviour
    {
        [Header("MAND")]
        //public string track_name;
        public GameObject UIStartTrackRef;
        public AudioSource startLineCrossed_SFX;
        public bool destroyOnActivation = true;

        private float countdownElapsedTime = 0f;

        public bool enable_tricks = false;
        public GameObject UIHandle; // for tricktracker

        public int countdown = 3;
        public bool is_rdy_to_launch = false;

        public CinematicTrigger entryCinematicTrigger;

        public AudioSource audioSource;
        public AudioClip countDownSFX0;
        public AudioClip countDownSFX1;

        [Header("Internal")]
        private PlayerController PC;

        private UIStartTrack UIStartTrackInst = null;
        // Start is called before the first frame update
        void Start()
        {
            countdownElapsedTime = 0f;
        }

        void FixedUpdate()
        {
            // if (UIStartTrackInst != null)
            // {
            //     countdownElapsedTime += Time.fixedDeltaTime;
            // }
        }

        public void launchCountdown()
        {
            if (UIStartTrackInst == null)
            {
                UIStartTrackInst = Instantiate(UIStartTrackRef).GetComponent<UIStartTrack>();
                countdownElapsedTime = 0f;

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

            PC.TransitionTo(PlayerController.PlayerVehicleStates.Car);

            if (enable_tricks)
            activateTricks();

            StartCoroutine(postCountdown());
        }

        IEnumerator countdownCo(PlayerController iPC)
        {
            iPC.Freeze();

            audioSource.clip = countDownSFX0;
            audioSource.Play(0);

            countdownElapsedTime = 0f;
            float secondElapsedTime = 0f;
            while (countdownElapsedTime < countdown)
            {
                countdownElapsedTime += Time.deltaTime;
                secondElapsedTime += Time.deltaTime;
                if (secondElapsedTime >= 1f)
                {
                    audioSource.clip = countDownSFX0;
                    audioSource.Play(0);
                    UIStartTrackInst.updateDisplay(countdownElapsedTime);

                    secondElapsedTime = 0f;
                }

                yield return null;
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
                PC = Access.Player();
                if (!is_rdy_to_launch)
                {
                    is_rdy_to_launch = Access.TrackManager().trackIsReady; // TODO  wait for scenemanager to tell its ok
                    PC.Freeze();
                }
                else
                {
                    if (entryCinematicTrigger != null)
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
            TrickTracker tt = PC.gameObject.GetComponent<TrickTracker>();
            if (!!tt)
            {
                tt.activate_tricks = true; // activate default in hub
                tt.init(UIHandle);
            }
        }
    }
}
