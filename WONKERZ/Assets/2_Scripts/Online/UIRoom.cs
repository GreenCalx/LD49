using Mirror;
using Schnibble;
using Schnibble.Managers;
using Schnibble.UI;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

// TODO: move this to the real network room.
// might even need to be something on the lobby server.
// Completoyl retarded to give the player, should be refactored ASAP
public struct UIPlayerSlotData
{
    public NetworkRoomPlayerExt player;
    public string name;
    public int latency;
    public bool readyState;
    public Color backgroundColor;
};

public class UIRoom : UIPanelTabbed
{
    // TODO: remove this! and set dircetly the lobby value.
    public static int roomPlayerCount = 4;

    public GameObject UIPlayerSlot_prefab;
    public GameObject UIEmptyPlayerSlot_prefab;

    List<UIRoom_PlayerSlot> uiPlayerSlots = new List<UIRoom_PlayerSlot>(roomPlayerCount);
    List<UIRoom_EmptySlot>  uiEmptySlots = new List<UIRoom_EmptySlot>(roomPlayerCount);

    UIRoom_PlayerSlot localPlayerSlot = null;

    UIOnline uiOnline;

    public RectTransform background;
    //public GameObject readyUpButtonPrefab;
    //GameObject readyUpButton = null;

    #pragma warning disable CS0414
    public bool showGameStartButton = false;
    public GameObject startGameButtonPrefab;
    GameObject startGameButton = null;
    #pragma warning restore CS0414

    // string based for now.
    static readonly string startHint = "Press ";
    static readonly string readyUpHintEnd = " to ready up.";
    static readonly string readyUpButton = "UIStart";

    public UILabel   readyUpHint;
    public UILabel   countdownHint;
    public UIElement fadingPanel;

    protected override void ProcessInputs(InputManager currentMgr, GameController Entry)
    {
        base.ProcessInputs(currentMgr, Entry);

        // start is ready button
        if (localPlayerSlot != null)
        {
            if (Entry.GetButtonState(Entry.GetIdx(readyUpButton)).down)
            {
                localPlayerSlot.roomPlayer.CmdChangeReadyState(!localPlayerSlot.roomPlayer.readyToBegin);
            }
        }
    }

    public override void Activate()
    {
        base.Activate();

        uiPlayerSlots = new List<UIRoom_PlayerSlot>(roomPlayerCount);

        if (uiOnline == null)
        {
            uiOnline = (parent as UIOnline);
        }

        uiOnline.roomServer.OnRoomServerPlayersReadyCB += OnAllPlayersReady;
        uiOnline.roomServer.OnRoomClientSceneChangedCB += OnGameLoaded;
        uiOnline.roomServer.OnShowPreGameCountdown     += OnShowPreGameCountdown;
        // use client enter/exit because they are called AFTER roomSlots has been updated.
        uiOnline.roomServer.OnRoomClientEnterCB += delegate () {UpdatePlayerSlots(uiOnline.roomServer.roomSlots); };
        uiOnline.roomServer.OnRoomClientExitCB += delegate () {UpdatePlayerSlots(uiOnline.roomServer.roomSlots); };

        readyUpHint.Hide();
        countdownHint.Hide();
        fadingPanel.Hide();

        UpdatePlayerSlots(uiOnline.roomServer.roomSlots);
    }

    public override void Deactivate()
    {
        base.Deactivate();

        if (uiOnline) {
            if (uiOnline.roomServer) {
                uiOnline.roomServer.OnRoomServerPlayersReadyCB -= OnAllPlayersReady;
                uiOnline.roomServer.OnRoomClientSceneChangedCB -= OnGameLoaded;
                uiOnline.roomServer.OnShowPreGameCountdown     -= OnShowPreGameCountdown;
                uiOnline.roomServer.OnRoomClientEnterCB -= delegate () {UpdatePlayerSlots(uiOnline.roomServer.roomSlots); };
                uiOnline.roomServer.OnRoomClientExitCB -= delegate () {UpdatePlayerSlots(uiOnline.roomServer.roomSlots); };
            }
        }

        foreach (UITab t in tabs)
        {
            if (t != null) Destroy(t.gameObject);
        }
        tabs.Clear();
    }

    protected override void CancelAfterConfirmation() {
        if (NetworkServer.activeHost)
        {
            uiOnline.roomServer.StopHost();
        }
        else if (NetworkClient.isConnected)
        {
            uiOnline.roomServer.StopClient();
        }

        base.CancelAfterConfirmation();
    }

    public override void Cancel() {
        // if player is ready, we don't cancel the panel,
        // we set state to Not ready.
        if (localPlayerSlot && localPlayerSlot.roomPlayer.readyToBegin)
        {
            localPlayerSlot.ChangeReadyState(!localPlayerSlot.roomPlayer.readyToBegin);
        }
        else
        {
            base.Cancel();
        }
    }

    public void OnGameLoaded()
    {
        this.Log("OnGameLoaded : isGameplayScene = " + Mirror.Utils.IsSceneActive(uiOnline.roomServer.GameplayScene));
        // Deactivate UX.
        if (NetworkClient.active && Mirror.Utils.IsSceneActive(uiOnline.roomServer.GameplayScene))
        {
            Deactivate();
            uiOnline.SetState(UIOnline.States.Deactivated);
        }
    }


    public void OnAllPlayersReady()
    {
        this.Log("OnAllPlayersReady");
    }

    void OnShowPreGameCountdown(bool show) {
        if (NetworkRoomManagerExt.singleton == null) {
            this.LogError("NetworkManager is null.");
            return;
        }

        if (NetworkRoomManagerExt.singleton.onlineRoomData == null) {
            this.LogError("OnlineRoomData is null.");
            return;
        }

        if (show)
        {
            if (NetworkRoomManagerExt.singleton.onlineRoomData.isServer)
            {
                StartCoroutine(CountdownBeforeStart_Server(5));
            }
            else
            {
                // time is timeout in seconds in case the clinet/server connection is fucked.
                StartCoroutine(CountdownBeforeStart_Client(5 * 3));
            }
        }
    }

    public IEnumerator CountdownBeforeStart_Server(int seconds) {
        var ogm = NetworkRoomManagerExt.singleton.onlineRoomData;
        ogm.preGameCountdownTime = seconds;

        countdownHint.Show();
        fadingPanel  .Show();

        float timeoutTime = seconds * 3;

        bool hasError = false;
        while (ogm.preGameCountdownTime > 0.0f) {
            ogm.preGameCountdownTime -= Time.deltaTime;

            countdownHint.content = ((int)(ogm.preGameCountdownTime)).ToString();

            if (!uiOnline.roomServer.allPlayersReady) {
                ogm.showPreGameCountdown = false;
                hasError = true;
                break;
            }

            yield return null;
        }

        countdownHint.Hide();
        fadingPanel  .Hide();

        if (!hasError) {
            // Stop showing the countdown.
            ogm.showPreGameCountdown = false;
            // Stop input processing.
            StopInputs();
            // Start the game scene.
            // It will handle the transition effects and loading screen.
            uiOnline.roomServer.StartGameScene();
        }

        yield break;
    }

    public IEnumerator CountdownBeforeStart_Client(float timeoutTime) {
        var ogm = NetworkRoomManagerExt.singleton.onlineRoomData;

        countdownHint.Show();
        fadingPanel  .Show();

        bool hasError = false;
        while (ogm.showPreGameCountdown) {
            timeoutTime -= Time.deltaTime;
            if (timeoutTime < 0.0f) {
                // TODO: show error message.
                hasError = true;
                break;
            }

            countdownHint.content = ((int)(ogm.preGameCountdownTime)).ToString();

            yield return null;
        }

        countdownHint.Hide();
        fadingPanel  .Hide();

        uiOnline.SetState(UIOnline.States.Deactivated);

        if (hasError) {
            this.LogError("Timeout on countdown.");
        }

        yield break;
    }

    public void UpdatePlayerSlots(List<NetworkRoomPlayer> roomSlots)
    {
        this.Log("UpdatePlayerSlots : " + roomSlots.Count);
        // TODO: Update already existing slots instead of creating again all slots.

        // Clean-up
        foreach (var s in uiPlayerSlots)
        {
            if (s != null && s.gameObject != null)
            {
                // clean-up callbacks to avoid dead objects being called.
                GameObject.Destroy(s.gameObject);
            }
        }
        uiPlayerSlots.Clear();

        foreach (var e in uiEmptySlots)
        {
            if (e != null && e.gameObject != null)
            {
                // clean-up callbacks to avoid dead objects being called.
                GameObject.Destroy(e.gameObject);
            }
        }
        uiEmptySlots.Clear();

        foreach (var t in tabs)
        {
            if (t != null && t.gameObject != null)
            {
                GameObject.Destroy(t.gameObject);
            }
        }
        tabs.Clear();

        // Create again all slots objects
        for (int i = 0; i < roomPlayerCount; ++i)
        {
            if (i < roomSlots.Count)
            {
                var slot = roomSlots[i];

                UIPlayerSlotData playerData;
                playerData.backgroundColor = UnityEngine.Random.ColorHSV();
                playerData.name = slot.name;
                playerData.readyState = slot.readyToBegin;
                playerData.player = slot as NetworkRoomPlayerExt;
                playerData.latency = -1;

                var uiSlot = AddPlayer(playerData);
                tabs.Add(uiSlot);

                if (slot.isLocalPlayer)
                {
                    localPlayerSlot = uiSlot;
                }
            } else {
                // empty players.
                var emptySlot = AddEmpty(i);
                tabs.Add(emptySlot);
            }
        }

        GetTab(CurrentTab()).Select();
    }

    public UIRoom_EmptySlot AddEmpty(int idx) {
        var result = Instantiate(UIEmptyPlayerSlot_prefab, this.gameObject.transform).GetComponent<UIRoom_EmptySlot>();
        result.parent = this;

        result.Init();
        result.Deselect();
        result.Show();
        // Get current window size.
        // TODO: move this to cache
        var uxSize = background.rect;
        int uxHeight = (int)uxSize.height;
        // TODO: take into account where the anchor is to compute the right position.
        // TODO: add possibility for padding.
        int uxWidth = (int)(uxSize.width);

        int slotAvailableWidth = (uxWidth / roomPlayerCount);
        int halfSlotAvailableWidth = slotAvailableWidth / 2;
        // Centered on 0 => remove uxWidth to go negative for first indices
        int xPos = slotAvailableWidth * (idx+1) - halfSlotAvailableWidth - (uxWidth / 2);
        result.gameObject.transform.localPosition = new Vector3(xPos, 0, 0);

        uiEmptySlots.Add(result);

        return result;
    }

    public UIRoom_PlayerSlot AddPlayer(UIPlayerSlotData data)
    {
        //var playerSlot = UIRoom_PlayerSlot.Create(this.gameObject, data.backgroundColor, data.name, data.readyState);
        var slot = GameObject.Instantiate(UIPlayerSlot_prefab, this.gameObject.transform).GetComponent<UIRoom_PlayerSlot>();

        slot.parent = this;
        slot.Init();
        slot.Deselect();

        slot.AttachRoomPlayer(data.player);

        slot.UpdateView();

        slot.Show();

        // Get current window size.
        // TODO: move this to cache
        var uxSize = background.rect;
        int uxHeight = (int)uxSize.height;
        // TODO: take into account where the anchor is to compute the right position.
        // TODO: add possibility for padding.
        int uxWidth = (int)(uxSize.width);
        // move to the right position
        int idx = uiPlayerSlots.Count + 1;

        int slotAvailableWidth = (uxWidth / roomPlayerCount);
        int halfSlotAvailableWidth = slotAvailableWidth / 2;

        int xPos = slotAvailableWidth * idx - halfSlotAvailableWidth - (uxWidth / 2);
        slot.SetPosition(xPos, 0);

        uiPlayerSlots.Add(slot);
        // might need to return the newly created object at some point?
        return slot;
    }

    public void OnLocalPlayerReadyStateChanged(bool newState)
    {
        if (readyUpHint != null)
        {
            if (!newState)
            {
                readyUpHint.content = startHint +
                                      // TODO: make this more robust in InputManager.
                                        inputMgr.controllers[0].controller.Get(readyUpButton).name +
                                    readyUpHintEnd;
                readyUpHint.Show();
            }
            else
            {
                readyUpHint.Hide();
            }
        }
    }

    // called from localPlayer slot when changing.
    // NOTE: note used anymore.o
#if false
    public void UpdateReadyStateButton(UIRoom_PlayerSlot slot) {
            if (slot.roomPlayer.readyToBegin) {
                if (readyUpButton != null) {
                    Destroy(readyUpButton.gameObject);
                }
            } else {
                if (readyUpButton == null) readyUpButton = Instantiate(readyUpButtonPrefab, this.gameObject.transform);

                readyUpButton.SetActive(true);

                var tab = readyUpButton.GetComponent<UITextTab>();
                tab.onActivate.AddListener(delegate { slot.ChangeReadyState(true); });

                if (!tabs.Contains(tab))
                {
                    tabs.Add(tab);
                    tab.init();
                    tab.deselect();
                }
            }
        }
#endif
}
