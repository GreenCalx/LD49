using UnityEngine;
namespace Wonkerz {
/*
* A Camera for UI only scenes
*/
public class UICamera : GameCamera
{
    protected override void Awake()
    {
        camType = CAM_TYPE.UI;
    }

    void OnEnable() {
        Access.managers.audioListenerMgr.SetListener(this.gameObject);
    }

    void OnDisable() {
        Access.managers.audioListenerMgr.UnsetListener(this.gameObject);
    }
}
}
