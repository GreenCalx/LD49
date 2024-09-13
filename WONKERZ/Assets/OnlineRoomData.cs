using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

// Use this to store sync data in OnlineRoom.
// It will not be usable in the GameScene, use OnlineGameManager for that.
public class OnlineRoomData : NetworkBehaviour
{
    // UX online data.
    [SyncVar]
    public float preGameCountdownTime = .0f;
    [SyncVar(hook=nameof(OnPreGameCountdown))]
    public bool showPreGameCountdown = false;

    void OnPreGameCountdown(bool oldValue, bool newValue) {
        NetworkRoomManagerExt.singleton.OnShowPreGameCountdown?.Invoke(newValue);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (NetworkRoomManagerExt.singleton != null) NetworkRoomManagerExt.singleton.onlineRoomData = this;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        if (NetworkRoomManagerExt.singleton !=null) NetworkRoomManagerExt.singleton.onlineRoomData = null;
    }
}
