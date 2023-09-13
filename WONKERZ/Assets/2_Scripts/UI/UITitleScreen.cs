using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;

public class UITitleScreen : UIPanelTabbed
{
    public UIPanelTabbed confirmExitPanel;
    void Start(){
        activate();
        cancelDeactivatePanel = false;
    }

    void OnDestroy()
    {
        deactivate();
    }
    protected override void ProcessInputs(InputManager.InputData entry){
        base.ProcessInputs(entry);

        if (entry.Inputs[GameInputsUtils.GetIdx(inputCancel)].IsDown)
        {
            // propose to quit to desktop
            confirmExitPanel.onActivate?.Invoke();
        }
    }

    public void launchNewGame()
    {
        Access.SceneLoader().loadScene(Constants.SN_INTRO);
    }

    public void launchLoadGame()
    {
        Access.CollectiblesManager().loadJars();
        Access.GameProgressSaveManager().Load();

        Access.SceneLoader().loadScene(Constants.SN_HUB);
    }

    public void quitGame(){
        Schnibble.Utils.ExitGame();
    }

    public void launchDemo()
    {
        Access.SceneLoader().loadScene(Constants.SN_DESERT_TOWER);
    }
}
