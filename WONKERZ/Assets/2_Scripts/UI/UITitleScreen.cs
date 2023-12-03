using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;

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

    protected override void ProcessInputs(InputManager currentMgr, GameInput[] entry){
        base.ProcessInputs(currentMgr, entry);

        if ((entry[PlayerInputs.GetIdx(inputCancel)] as GameInputButton).GetState().down)
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
        Access.SceneLoader().loadScene(Constants.SN_DESERT_TOWER);
    }
}
