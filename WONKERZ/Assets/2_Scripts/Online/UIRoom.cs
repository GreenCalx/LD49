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
    public static int roomPlayerCount = 4;

    public GameObject UIPlayerSlot_prefab;
    List<UIRoom_PlayerSlot> uiPlayerSlots = new List<UIRoom_PlayerSlot>(roomPlayerCount);
    UIRoom_PlayerSlot localPlayerSlot = null;

    UIOnline uiOnline;

    public RectTransform background;
    //public GameObject readyUpButtonPrefab;
    //GameObject readyUpButton = null;

    public bool showGameStartButton = false;
    public GameObject startGameButtonPrefab;
    GameObject startGameButton = null;

    bool isGameLoading = false;

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

    public override void activate()
    {
        base.activate();

        uiPlayerSlots = new List<UIRoom_PlayerSlot>(roomPlayerCount);

        if (uiOnline == null)
        {
            uiOnline = (Parent as UIOnline);
        }

        uiOnline.roomServer.OnRoomServerPlayersReadyCB += OnAllPlayersReady;
        uiOnline.roomServer.OnRoomClientSceneChangedCB += OnGameLoaded;
        uiOnline.roomServer.OnShowPreGameCountdown += OnShowPreGameCountdown;

        readyUpHint.Hide();
        countdownHint.Hide();
        fadingPanel.Hide();

        UpdatePlayerSlots(uiOnline.roomServer.roomSlots);
    }

    public override void deactivate()
    {
        base.deactivate();

        uiOnline.roomServer.OnRoomServerPlayersReadyCB -= OnAllPlayersReady;
        uiOnline.roomServer.OnRoomClientSceneChangedCB -= OnGameLoaded;

        foreach (UITab t in tabs)
        {
            if (t != null) Destroy(t.gameObject);
        }
        tabs.Clear();
    }

    public override void cancel()
    {
        // if player is ready, we don't cancel the panel,
        // we set state to Not ready.
        if (localPlayerSlot && localPlayerSlot.roomPlayer.readyToBegin)
        {
            localPlayerSlot.ChangeReadyState(!localPlayerSlot.roomPlayer.readyToBegin);
        }
        else
        {
            base.cancel();

            if (NetworkServer.activeHost)
            {
                uiOnline.roomServer.StopHost();
            }
            else if (NetworkClient.isConnected)
            {
                uiOnline.roomServer.StopClient();
            }
        }
    }

    public void OnGameLoaded()
    {
        this.Log("OnGameLoaded : isGameplayScene = " + Mirror.Utils.IsSceneActive(uiOnline.roomServer.GameplayScene));
        // Deactivate UX.
        if (NetworkClient.active && Mirror.Utils.IsSceneActive(uiOnline.roomServer.GameplayScene))
        {
            uiOnline.SetState(UIOnline.States.Deactivated);
        }
    }

    public void OnAllPlayersReady()
    {
        this.Log("OnAllPlayersReady");
        // Start game
        if (showGameStartButton)
        {
            // TODO: Show start button to start the game on host
        }
        else
        {
        }
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

            countdownHint.text.text = ((int)(ogm.preGameCountdownTime)).ToString();

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

            countdownHint.text.text = ((int)(ogm.preGameCountdownTime)).ToString();

            yield return null;
        }

        countdownHint.Hide();
        fadingPanel  .Hide();

        yield break;
    }

    public void UpdatePlayerSlots(List<NetworkRoomPlayer> roomSlots)
    {
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

        foreach (var t in tabs)
        {
            if (t != null && t.gameObject != null)
            {
                GameObject.Destroy(t.gameObject);
            }
        }
        tabs.Clear();

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
            tabs.Add(uiSlot);

            if (slot.isLocalPlayer)
            {
                localPlayerSlot = uiSlot;
            }
        }
    }

    public UIRoom_PlayerSlot AddPlayer(UIPlayerSlotData data)
    {
        //var playerSlot = UIRoom_PlayerSlot.Create(this.gameObject, data.backgroundColor, data.name, data.readyState);
        var slot = GameObject.Instantiate(UIPlayerSlot_prefab, this.gameObject.transform).GetComponent<UIRoom_PlayerSlot>();

        slot.Parent = this;
        slot.init();

        slot.AttachRoomPlayer(data.player);

        slot.UpdateView();
        slot.gameObject.SetActive(true);

        // Get current window size.
        // TODO: move this to cache
        var uxSize = background.rect;
        int uxHeight = (int)uxSize.height;
        int uxWidth = (int)(uxSize.width - slot.background.GetComponent<RectTransform>().rect.width);
        // move to the right position
        int idx = uiPlayerSlots.Count;

        int xPos = (uxWidth / roomPlayerCount) * idx - (uxWidth / 2);
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
                readyUpHint.text.text = startHint +
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
