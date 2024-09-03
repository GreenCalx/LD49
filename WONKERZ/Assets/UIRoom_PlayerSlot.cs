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

    public NetworkRoomPlayerExt roomPlayer;

    public void OnReadyStateChanged(bool oldState, bool newState ) {
        SetReadyState(newState);

        UnityEngine.Debug.Log("OnReadyStateChanged :" + newState);

        if (roomPlayer.isLocalPlayer) {
            UnityEngine.Debug.Log("OnReadyStateChanged localPlayer:" + roomPlayer.isLocalPlayer);
            this.gameObject.transform.parent.GetComponent<UIRoom>().showReadyUpButton(!newState, this);
        }
    }

    public void ChangeReadyState() {
        roomPlayer.CmdChangeReadyState(true);
    }

    public void OnUpdateLatency(int latency) {
        SetLatency(latency);
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
