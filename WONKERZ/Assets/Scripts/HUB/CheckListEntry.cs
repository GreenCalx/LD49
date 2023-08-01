using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;

public class CheckListEntry : MonoBehaviour, IControllable
{
    public GameObject uiCheckListRef;
    public PlayerDetector detector;

    private GameObject uiCheckList;

    private bool playerInCheckList;
    private bool checkListOpened;

    private Vector3 playerPositionWhenEntered;
    private Transform playerTransform;

    void Start()
    {
        playerInCheckList = false;
        Utils.attachControllable<CheckListEntry>(this);
    }

    void OnDestroy()
    {
        Utils.detachControllable<CheckListEntry>(this);
    }

    void IControllable.ProcessInputs(InputManager.InputData Entry)
    {
        if (detector.playerInRange)
        {
            if (Entry.Inputs[(int)GameInputsButtons.Jump].IsDown)
            open();
        }
    }

    public void open()
    {
        if (checkListOpened)
            return;

        uiCheckList = Instantiate(uiCheckListRef);
        uiCheckList.SetActive(true);

        UIBountyMatrix uibm = uiCheckList.GetComponentInChildren<UIBountyMatrix>();
        uibm.onActivate.Invoke();
        uibm.onDeactivate.AddListener(close);

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
    }
}
