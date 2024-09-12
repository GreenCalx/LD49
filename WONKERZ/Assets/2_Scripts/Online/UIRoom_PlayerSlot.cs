using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Schnibble.UI;

public class UIRoom_PlayerSlot : UITab
{
    public GameObject background;
    public GameObject latency;
    public GameObject playerName;

    public GameObject readyState;

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
        SetReadyState(roomPlayer.readyToBegin);
        if (roomPlayer.isLocalPlayer) {
            (Parent as UIRoom).UpdateReadyStateButton(this);
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
        this.background.GetComponent<Image>().color = c;
    }

    public void SetPlayerName(string playerName) {
        this.playerName.GetComponent<TextMeshProUGUI>().text = playerName;
    }

    public void SetReadyState(bool readyState) {
        this.readyState.GetComponent<TextMeshProUGUI>().text = readyState ? "Lock and loaded!" : "Wait for me!";
    }

    public void SetLatency(int latency) {
        this.latency.GetComponent<TextMeshProUGUI>().text = latency.ToString() + " ms";
    }
}
