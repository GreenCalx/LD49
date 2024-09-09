using UnityEngine;
using Mirror;
using System;
using System.Collections;
using Schnibble;

public class NetworkRoomPlayerExt : NetworkRoomPlayer
{
    public struct RoomPlayerInfos
    {
        public string playerName;
        public Color backgroundColor;
        public double rtt;
    };

    [SyncVar]
    public RoomPlayerInfos _infos;
    public RoomPlayerInfos infos { get => _infos; set { _infos = value; onAnyChange?.Invoke(); } }

    public Action onAnyChange;
    // Maintain latency up to date.
    // Because it is only available on the server.
    // For now it means we will send all data instead of just syncing the rtt
    // but it should be fine.
    Coroutine coro_UpdateLatency;
    IEnumerator UpdateLatency() {
        if (!isServer)
        {
            // Should not be used on a client only.
            this.LogError("Trying to use server only function but isServer is false. \n Latency will no be updated.");
            yield break;
        }

        while (true) {
            RoomPlayerInfos newInfos = new RoomPlayerInfos();
            newInfos.playerName = name;
            newInfos.backgroundColor = _infos.backgroundColor;
            newInfos.rtt = connectionToClient.rtt;
            infos = newInfos;
            yield return new WaitForSeconds(0.2f);
        }

        yield break;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _infos.backgroundColor = UnityEngine.Random.ColorHSV();

        coro_UpdateLatency = StartCoroutine(UpdateLatency());
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        if (coro_UpdateLatency != null)
        {
            StopCoroutine(coro_UpdateLatency);
            coro_UpdateLatency = null;
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();

        if (coro_UpdateLatency != null)
        {
            StopCoroutine(coro_UpdateLatency);
            coro_UpdateLatency = null;
        }
    }

    public override void OnClientEnterRoom()
    {
        gameObject.name = "Room" + Constants.GO_PLAYER + this.index.ToString();
    }

    public override void IndexChanged(int oldIndex, int newIndex)
    {
        gameObject.name = "Room" + Constants.GO_PLAYER + this.index.ToString();
    }

    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
    {
        onAnyChange?.Invoke();
    }
}

