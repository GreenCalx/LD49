using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIRoom_PlayerSlot : MonoBehaviour
{
    public GameObject background;
    public GameObject latency;
    public GameObject playerName;
    public GameObject readyState;

    NetworkRoomPlayerExt roomPlayer;
    public void AttachRoomPlayer(NetworkRoomPlayerExt player) {
        if (roomPlayer != null && roomPlayer != player) {
            roomPlayer.onAnyChange -= UpdateView;
        }
        roomPlayer = player;
        if (roomPlayer != null) {
            roomPlayer.onAnyChange += UpdateView;
        }
    }

    void OnDestroy() {
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
        if (!roomPlayer.readyToBegin) {
            // Player is not ready,
            // for now we only show a button if we are the current player that is not ready.
            if (roomPlayer.isLocalPlayer) {
                // TODO: better child/parent.
                this.gameObject.transform.parent.GetComponent<UIRoom>().showReadyUpButton(!roomPlayer.readyToBegin, this);
            }
        }
    }

    public void ChangeReadyState() {
        roomPlayer.CmdChangeReadyState(true);
    }

    public static UIRoom_PlayerSlot Create(GameObject parent, Color backgroundColor, string playerName, bool readyState) {
        UIRoom_PlayerSlot slot = parent.AddComponent<UIRoom_PlayerSlot>();

        slot.SetBackgroundColor(backgroundColor);
        slot.SetPlayerName(playerName);
        slot.SetReadyState(readyState);
        return slot;
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
