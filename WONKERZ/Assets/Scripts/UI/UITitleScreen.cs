using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITitleScreen : UIPanelTabbed
{
    void Start(){
        activate();
    }

    void OnDestroy()
    {
        deactivate();
    }
    public void launchNewGame()
    {
        Access.SceneLoader().loadScene(Constants.SN_INTRO);
    }

    public void launchLoadGame()
    {
        Access.CollectiblesManager().loadJars();
        Access.SceneLoader().loadScene(Constants.SN_HUB);
    }
}
