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
        if (!!iCol.GetComponent<CarController>() && !playerInCheckList)
        {
            playerInCheckList = true;
            var SndMgr = Access.SoundManager();
            SndMgr.SwitchClip("garage");
        }
    }

    void OnTriggerExit(Collider iCol)
    {
        if (!!iCol.GetComponent<CarController>() && playerInCheckList)
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

        CarController cc = Access.Player();
        cc.stateMachine.ForceState(cc.frozenState);
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

        CarController cc = Access.Player();
        cc.stateMachine.ForceState(cc.aliveState);
        checkListOpened = false;
    }
}
