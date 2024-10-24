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
    public UIPanelTabbed  nameEntryPanel;
    public UIProfileCards profilePanel;

    protected override void OnEnable()
    {
        inputMgr = Access.managers.playerInputsMgr.player1;

        base.OnEnable();

        // NOTE: if we come back from online, we make sure to delete the network manager.
        if (NetworkRoomManagerExt.singleton != null)
        {
            SceneManager.MoveGameObjectToScene(NetworkRoomManagerExt.singleton.gameObject.transform.root.gameObject, SceneManager.GetActiveScene());
            GameObject.Destroy(NetworkRoomManagerExt.singleton.gameObject);
        }

        Init();
        Activate();
        Show();

        Access.managers.fpsLimiter.LimitFPS(true);
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
        // check if we are using a profile.
        var gameSaveManager = Access.managers.gameProgressSaveMgr;
        // just to be sure
        gameSaveManager.onProfileLoaded -= launchOnline;
        gameSaveManager.onProfileLoaded -= launchLocal;

        if (!gameSaveManager.HasActiveProfile()) {
            // No profile selected => show profile selection/creation.
            profilePanel.Show();
            gameSaveManager.onProfileLoaded += launchOnline;
            return;
        }

        StopInputs();

        // TODO: do not remove player but switch between local, online, etc...
        // There should probably be no player at all on the titleScreen anyway.
        PlayerController pc = Access.Player();
        if (!!pc)
        {
            Destroy(pc.gameObject);
            var mainCam = Access.managers.cameraMgr.active_camera.cam;
            if (mainCam != null)
            {
                AudioListener al = null;
                if (!mainCam.gameObject.TryGetComponent<AudioListener>(out al))
                {
                    al = mainCam.gameObject.AddComponent<AudioListener>();
                }

                if (al == null) {
                    this.LogError("Weird shit.");
                }

                al.enabled = true;
            }
        }

        Access.managers.gameSettings.isLocal = false;
        Access.managers.sceneMgr.loadScene("OfflineRoom", new SceneLoader.LoadParams{
            useLoadingScene = true,
            useTransitionIn = true,
            useTransitionOut = true,
        });
    }

    public void launchLocal() {
        // check if we are using a profile.
        var gameSaveManager = Access.managers.gameProgressSaveMgr;
        // just to be sure...
        gameSaveManager.onProfileLoaded -= launchLocal;
        gameSaveManager.onProfileLoaded -= launchOnline;

        if (!gameSaveManager.HasActiveProfile()) {
            // No profile selected => show profile selection/creation.
            profilePanel.Show();
            gameSaveManager.onProfileLoaded += launchLocal;
            return;
        }

        StopInputs();

        PlayerController pc = Access.Player();
        if (!!pc)
        {
            Destroy(pc.gameObject);
            var mainCam = Access.managers.cameraMgr.active_camera.cam;
            if (mainCam != null)
            {
                AudioListener al = null;
                if (!mainCam.gameObject.TryGetComponent<AudioListener>(out al))
                {
                    al = mainCam.gameObject.AddComponent<AudioListener>();
                }

                if (al == null) {
                    this.LogError("Weird shit.");
                }

                al.enabled = true;
            }
        }

        Access.managers.gameSettings.isLocal = true;

        Access.managers.sceneMgr.loadScene("OfflineRoom", new SceneLoader.LoadParams{
            useLoadingScene = true,
            useTransitionIn = true,
            useTransitionOut = true,
        });
    }

    public void launchDemo()
    {
        this.LogError("Not implemented");
    }

    public override void Cancel() {
        base.Cancel();
        defaultCancelPanel.reason.content = "You are about to exit,";
    }
}
