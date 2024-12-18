using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;
using Schnibble.Managers;

namespace Wonkerz
{

    public class CheckListEntry : MonoBehaviour, IControllable
    {
        public GameObject uiCheckListRef;
        public PlayerDetector detector;

        public CinematicCamera checkListZoomCamera;
        public WonkerDecal interactibleZoneDecal;

        public float delayToShowUI = 0.5f;

        private GameObject uiCheckList;

        private bool checkListOpened;

        private Vector3 playerPositionWhenEntered;
        private Transform playerTransform;


        void Start()
        {
            Access.Player()?.inputMgr?.Attach(this as IControllable);
        }

        void OnDestroy()
        {
            Access.Player()?.inputMgr?.Detach(this as IControllable);
        }

        void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
        {
            if (detector.playerInRange)
            {
                if ((Entry.Get((int)PlayerInputs.InputCode.UIValidate) as GameInputButton).GetState().down)
                open();
            }
        }

        public void open()
        {
            if (checkListOpened)
            return;

            checkListZoomCamera.gameObject.SetActive(true);
            checkListZoomCamera.launch();

            if (delayToShowUI > 0f)
            {
                StartCoroutine(delayedUIShow());
            }
            else
            {
                showUI();
            }


            PlayerController player = Access.Player();
            playerTransform = player.transform;
            playerPositionWhenEntered = player.transform.position;
            player.Freeze();
            checkListOpened = true;
        }

        public void close()
        {
            if (!checkListOpened)
            return;

            checkListZoomCamera.end();
            checkListZoomCamera.gameObject.SetActive(false);

            var SndMgr = Access.SoundManager();
            SndMgr.SwitchClip("theme");

            Destroy(uiCheckList);

            PlayerController player = Access.Player();
            player.UnFreeze();

            checkListOpened = false;
            playerPositionWhenEntered = Vector3.zero;
        }

        void Update()
        {
            if (checkListOpened)
            {
                playerTransform.position = playerPositionWhenEntered;
            }

            if (!detector.playerInRange)
            {
                //elapsedInteractibleAnim += Time.deltaTime;
                interactibleZoneDecal.SetAnimationTime(1f);
            }

        }

        private void showUI()
        {
            uiCheckList = Instantiate(uiCheckListRef);
            uiCheckList.SetActive(true);

            UIBountyMatrix uibm = uiCheckList.GetComponentInChildren<UIBountyMatrix>();
            //uibm.onActivate.Invoke();
            uibm.onDeactivate.AddListener(close);
        }

        IEnumerator delayedUIShow()
        {
            float elapsed_time = 0f;
            while (elapsed_time < delayToShowUI)
            { elapsed_time += Time.deltaTime; yield return null; }
            showUI();
        }
    }
}
