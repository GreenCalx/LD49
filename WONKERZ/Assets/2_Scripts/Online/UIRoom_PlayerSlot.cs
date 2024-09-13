using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Schnibble.UI;

public class UIRoom_PlayerSlot : UITab
{
    public Image   background;
    public UILabel latency;
    public UILabel playerName;
    public UILabel readyState;
    public UIElement hostCrown;

    public NetworkRoomPlayerExt roomPlayer {get; private set; }
    public void AttachRoomPlayer(NetworkRoomPlayerExt player) {
        if (roomPlayer != null && roomPlayer != player) {
            roomPlayer.onAnyChange -= UpdateView;
        }
        roomPlayer = player;
        if (roomPlayer != null) {
            roomPlayer.onAnyChange += UpdateView;
        }
    }

    public override void init()
    {
        base.init();

        hostCrown.Hide();
    }

    override protected void OnDestroy() {
        if(roomPlayer != null)
        roomPlayer.onAnyChange -= UpdateView;
    }

    public void UpdateView() {
        if (roomPlayer == null) return;
        // pull player infos.
        SetBackgroundColor(roomPlayer.infos.backgroundColor);
        SetPlayerName(roomPlayer.infos.playerName);
        SetLatency((int)(roomPlayer.infos.rtt * 1000));
        // pull player states.
        // TODO: only react to the change instead of every frame.
        SetReadyState(roomPlayer.readyToBegin);

        // host : can we know without using the diagnostics index?
        if (roomPlayer.index == 0) {
            hostCrown.Show();
        } else {
            hostCrown.Hide();
        }

        if (roomPlayer.isLocalPlayer) {
            (Parent as UIRoom).OnLocalPlayerReadyStateChanged(roomPlayer.readyToBegin);
        }
    }

    public void ChangeReadyState(bool value) {
        roomPlayer.CmdChangeReadyState(value);
    }

    public void SetPosition(int x, int y) {
        //this.gameObject.GetComponent<RectTransform>().position = new Vector3(x, y, 0);
        this.gameObject.transform.localPosition = new Vector3(x, y, 0);
    }

    public void SetBackgroundColor(Color c) {
        this.background.color = c;
    }

    public void SetPlayerName(string playerName) {
        this.playerName.text.text = playerName;
    }

    public void SetReadyState(bool readyState) {
        this.readyState.text.text = readyState ? "Lock and loaded!" : "Wait for me!";
    }

    public void SetLatency(int latency) {
        this.latency.text.text = latency.ToString() + " ms";
    }
}
