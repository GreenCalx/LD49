using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;

public class CheckListEntry : MonoBehaviour, IControllable
{
    public GameObject uiCheckListRef;
    public PlayerDetector detector;

    public CinematicCamera checkListZoomCamera;
    public WonkerDecal interactibleZoneDecal;

    public float delayToShowUI = 0.5f;

    private GameObject uiCheckList;

    private bool playerInCheckList;
    private bool checkListOpened;

    private Vector3 playerPositionWhenEntered;
    private Transform playerTransform;


    private float elapsedInteractibleAnim = 0f;
    void Start()
    {
        playerInCheckList = false;
        Utils.attachControllable<CheckListEntry>(this);
    }

    void OnDestroy()
    {
        Utils.detachControllable<CheckListEntry>(this);
    }

    void IControllable.ProcessInputs(InputManager currentMgr, GameInput[] Entry)
    {
        if (detector.playerInRange)
        {
            if ((Entry[(int)PlayerInputs.InputCode.Jump] as GameInputButton).GetState().down)
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
        } else {
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
        uibm.onActivate.Invoke();
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
