using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Schnibble.UI;

// TODO: move this to the real network room.
// might even need to be something on the lobby server.
// Completoyl retarded to give the player, should be refactored ASAP
public struct UIPlayerSlotData
{
    public NetworkRoomPlayerExt player;
    public string name;
    public int    latency;
    public bool   readyState;
    public Color  backgroundColor;
};

public class UIRoom : UIPanelTabbed
{
    public static int roomPlayerCount = 4;

    public GameObject UIPlayerSlot_prefab;
    List<UIRoom_PlayerSlot> uiPlayerSlots = new List<UIRoom_PlayerSlot>(roomPlayerCount);

    UIOnline uiOnline;

    public RectTransform background;
    public GameObject readyUpButton;

    public bool showGameStartButton = false;
    public GameObject startGameButton;

    Coroutine coro_updateLatency;

    void Start() {
        uiOnline = (Parent as UIOnline);
    }

    public override void activate()
    {
        base.activate();

        if (uiOnline == null) Start();

        coro_updateLatency = StartCoroutine(UpdateLatency());

        uiOnline.roomServer.OnRoomServerPlayersReadyCB += OnAllPlayersReady;
        uiOnline.roomServer.OnRoomClientSceneChangedCB += OnGameLoaded;
    }

    public void OnGameLoaded() {
        // Deactivate UX.
        if (NetworkClient.active && Utils.IsSceneActive(uiOnline.roomServer.GameplayScene)) {
            uiOnline.SetState(UIOnline.States.Deactivated);
        }
    }

    public void OnAllPlayersReady() {
        UnityEngine.Debug.Log("OnAllPlayersReady");
        // Start game
        if (showGameStartButton) {
            // TODO: Show start button to start the game on host
        } else {
            uiOnline.roomServer.StartGameScene();
        }
    }

    public void OnPlayerConnected() {
        if (uiOnline == null) return;
        UnityEngine.Debug.Log("OnPlayerConnected: " + uiOnline.roomServer.roomSlots.Count);
        UpdatePlayerSlots(uiOnline.roomServer.roomSlots);
    }

    public void OnPlayerDisconnected() {
        if (uiOnline == null) return;
        UnityEngine.Debug.Log("OnPlayerDisconnected: " + uiOnline.roomServer.roomSlots.Count);
        UpdatePlayerSlots(uiOnline.roomServer.roomSlots);
    }

    public void UpdatePlayerSlots(List<NetworkRoomPlayer> roomSlots) {
        // TODO: Update already existing slots instead of creating again all slots.

        // Clean-up
        if (coro_updateLatency != null)
        {
            StopCoroutine(coro_updateLatency);
            coro_updateLatency = null;
        }
        foreach (var s in uiPlayerSlots)
        {
            // clean-up callbacks to avoid dead objects being called.
            s.roomPlayer.OnReadyStateChangedCB -= s.OnReadyStateChanged;
            GameObject.Destroy(s.gameObject);
        }
        uiPlayerSlots.Clear();

        // Create again all slots objects
        for (int i = 0; i < roomSlots.Count; ++i)
        {
            var slot = roomSlots[i];

            UIPlayerSlotData playerData;
            playerData.backgroundColor = UnityEngine.Random.ColorHSV();
            playerData.name = slot.name;
            playerData.readyState = slot.readyToBegin;
            playerData.player = slot as NetworkRoomPlayerExt;
            playerData.latency = -1;

            var uiSlot = AddPlayer(playerData);

            (slot as NetworkRoomPlayerExt).OnReadyStateChangedCB += uiSlot.OnReadyStateChanged;
        }
        // start again
        coro_updateLatency = StartCoroutine(UpdateLatency());
    }

    public UIRoom_PlayerSlot AddPlayer(UIPlayerSlotData data) {
        //var playerSlot = UIRoom_PlayerSlot.Create(this.gameObject, data.backgroundColor, data.name, data.readyState);
        var slot = GameObject.Instantiate(UIPlayerSlot_prefab, this.gameObject.transform).GetComponent<UIRoom_PlayerSlot>();

        slot.roomPlayer = data.player;

        slot.SetBackgroundColor(data.backgroundColor);
        slot.SetPlayerName(data.name);
        slot.OnReadyStateChanged(data.readyState, data.readyState);
        slot.OnUpdateLatency(data.latency);

        slot.gameObject.SetActive(true);

        // Get current window size.
        // TODO: move this to cache
        var uxSize = background.rect;
        int uxHeight = (int)uxSize.height;
        int uxWidth  = (int)(uxSize.width - slot.background.GetComponent<RectTransform>().rect.width);
        // move to the right position
        int idx = uiPlayerSlots.Count;

        int xPos = (uxWidth / roomPlayerCount) * idx - (uxWidth/2);
        slot.SetPosition(xPos, 0);

        uiPlayerSlots.Add(slot);
        // might need to return the newly created object at some point?
        return slot;
    }

    IEnumerator UpdateLatency() {
        while (true) {
            for(int i =0; i < uiPlayerSlots.Count ; ++i) {
                var slot = uiPlayerSlots[i];
                int latency = (int)(slot.roomPlayer.rtt * 1000);
                slot.OnUpdateLatency(latency);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void showReadyUpButton(bool show, UIRoom_PlayerSlot slot) {
        readyUpButton.SetActive(show);
        if (show) {
            var tab = readyUpButton.GetComponent<UITextTab>();
            tab.onActivate.AddListener(slot.ChangeReadyState);

            this.tabs.Add(tab);
        }
    }
}
