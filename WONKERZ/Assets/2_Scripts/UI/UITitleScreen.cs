using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;
using Schnibble.UI;
using Schnibble.Managers;
using Wonkerz; // TODO : include me in namespace

public class UITitleScreen : UIPanelTabbed
{
    public UIPanelTabbed confirmExitPanel;
    public UIPanelTabbed nameEntryPanel;
    public UIProfileCards profilePanel;

    
    void Start()
    {
        inputMgr = Access.PlayerInputsManager().player1;

        activate();
        cancelDeactivatePanel = false;
    }

    void OnDestroy()
    {
        deactivate();
    }
    protected override void ProcessInputs(InputManager currentMgr, GameController entry){
        base.ProcessInputs(currentMgr, entry);

        if ((entry.Get(PlayerInputs.GetIdx(inputCancel)) as GameInputButton).GetState().down)
        {
            // propose to quit to desktop
            confirmExitPanel.onActivate?.Invoke();
        }
    }

    public void launchNewGame()
    {
        nameEntryPanel.onActivate?.Invoke();

        profilePanel.mode = UIProfileCards.FD_MODE.WRITE;
        // profilePanel.onActivate?.Invoke();

        // Access.SceneLoader().loadScene(Constants.SN_INTRO);
    }

    public void launchLoadGame()
    {
        // Access.CollectiblesManager().loadJars();
        // Access.GameProgressSaveManager().Load();

        // Access.SceneLoader().loadScene(Constants.SN_HUB);
        profilePanel.mode = UIProfileCards.FD_MODE.READ;
        profilePanel.onActivate?.Invoke();
    }

    public void quitGame(){
        Schnibble.Utils.ExitGame();
    }

    public void launchDemo()
    {
        PlayerController pc = Access.Player();
        if (!!pc)
            Destroy(pc.gameObject);
        Access.SceneLoader().loadScene("OfflineRoom");
    }
}
