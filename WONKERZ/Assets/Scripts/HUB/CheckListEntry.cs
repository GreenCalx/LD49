using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;

public class CheckListEntry : MonoBehaviour, IControllable
{
    public GameObject uiCheckListRef;
    private GameObject uiCheckList;

    private bool playerInCheckList;
    private bool checkListOpened;

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
        if (playerInCheckList)
        {
            if (Entry.Inputs[Constants.INPUT_JUMP].IsDown)
                open();
        }
    }

    void OnTriggerEnter(Collider iCol)
    {
        if (Utils.colliderIsPlayer(iCol) && !playerInCheckList)
        {
            playerInCheckList = true;
            var SndMgr = Access.SoundManager();
            SndMgr.SwitchClip("garage");
        }
    }

    void OnTriggerExit(Collider iCol)
    {
        if (Utils.colliderIsPlayer(iCol) && playerInCheckList)
        {
            close();
            playerInCheckList = false;

            var SndMgr = Access.SoundManager();
            SndMgr.SwitchClip("theme");
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
        player.Freeze();
        checkListOpened = true;
    }

    public void close()
    {
        if (!checkListOpened)
            return;
        
        //uiCheckList.SetActive(true);
        UIBountyMatrix uibm = uiCheckList.GetComponentInChildren<UIBountyMatrix>();
        //uibm.onDeactivate.Invoke();

        Destroy(uiCheckList);

        PlayerController player = Access.Player();
        player.UnFreeze();

        checkListOpened = false;
    }
}
