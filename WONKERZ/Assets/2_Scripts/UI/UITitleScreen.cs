using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;
using Schnibble.UI;
using Schnibble.Managers;
using Wonkerz; // TODO : include me in namespace
using UnityEngine.SceneManagement;

public class UITitleScreen : UIPanelTabbed
{
    public UIPanelTabbed confirmExitPanel;
    public UIPanelTabbed nameEntryPanel;
    public UIProfileCards profilePanel;

    protected override void OnEnable()
    {
        inputMgr = Access.PlayerInputsManager().player1;

        base.OnEnable();

        // NOTE: if we come back from online, we make sure to delete the network manager.
        if (NetworkRoomManagerExt.singleton != null)
        {
            SceneManager.MoveGameObjectToScene(NetworkRoomManagerExt.singleton.gameObject.transform.root.gameObject, SceneManager.GetActiveScene());
            GameObject.Destroy(NetworkRoomManagerExt.singleton.gameObject);
        }
    }

    public override void cancel()
    {
        base.cancel();

        //open confirm panel.
        confirmExitPanel.activate();
    }

    public void launchNewGame()
    {
        nameEntryPanel.onActivate?.Invoke();

        profilePanel.mode = UIProfileCards.FD_MODE.WRITE;
    }

    public void launchLoadGame()
    {
        profilePanel.mode = UIProfileCards.FD_MODE.READ;
        profilePanel.onActivate?.Invoke();
    }

    public void quitGame(){
        Schnibble.Utils.ExitGame();
    }

    public void launchOnline() {
        PlayerController pc = Access.Player();
        if (!!pc) Destroy(pc.gameObject);

        Access.GameSettings().isLocal = false;
        Access.SceneLoader().loadScene("OfflineRoom");
    }

    public void launchLocal() {
        PlayerController pc = Access.Player();
        if (!!pc) Destroy(pc.gameObject);

        Access.GameSettings().isLocal = true;

        Access.SceneLoader().loadScene("OfflineRoom");
    }

    public void launchDemo()
    {
        this.LogError("Not implemented");
    }
}
