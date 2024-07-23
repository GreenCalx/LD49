using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SchOnline_SyncGameObjectState : NetworkBehaviour
{
    bool isHost => isServer && isClient;
    void OnEnable() {
        // Send this to every clients.
        if (isServer) {
            RpcSetActive(true);
        }
    }

    void OnDisable() {
        // Send this to every clients.
        if (isServer) {
            RpcSetActive(false);
        }
    }

    [ClientRpc]
    void RpcSetActive(bool active) {
        // Avoid circular recursive calls when we are the host.
        if (isHost) return;

        this.gameObject.SetActive(active);
    }
}
