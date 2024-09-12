using UnityEngine;
namespace Wonkerz {
/*
* A Camera for UI only scenes
*/
public class UICamera : GameCamera
{
    void Awake()
    {
        camType = CAM_TYPE.UI;
    }

    void OnEnable() {
        Access.GetMgr<AudioListenerManager>().SetListener(this.gameObject);
    }

    void OnDisable() {
        Access.GetMgr<AudioListenerManager>().UnsetListener(this.gameObject);
    }
}
}
