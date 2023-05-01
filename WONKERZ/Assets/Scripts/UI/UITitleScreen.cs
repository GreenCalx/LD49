using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITitleScreen : UIGaragePanel
{
    protected override void Awake()
    {
        base.Awake();
        inputMgr = Access.InputManager();
        Utils.attachControllable(this);
    }

    void OnDestroy()
    {
        Utils.detachControllable(this);
    }

    override public void deactivate()
    {
        base.deactivate();
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
