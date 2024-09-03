using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Schnibble;
using Schnibble.UI;
using Schnibble.Online;
using System.Net;
using System.Net.Sockets;

public class UILobbyServerList : UIPanelTabbedScrollable
{
    public VerticalLayoutGroup layout;

    public UILobbyServerTab lobbyTabObject;

    public UIOnline online { get; private set; }

    public class DisplayRoomListCmd : UIOnline.MainThreadCommand
    {
        List<Lobby> lobbies;
        UILobbyServerList ui;
        public DisplayRoomListCmd(List<Lobby> lobbies, UILobbyServerList ui)
        {
            this.lobbies = lobbies;
            this.ui = ui;
        }

        public override void Do()
        {
            foreach (var lobby in lobbies)
            {
                ui.CreateLobbyTab(lobby);
            }

            ui.SelectTab(0);

            OnCmdSuccess?.Invoke();
        }
    };

    public override void activate()
    {
        base.activate();

        if (Parent == null) {
            UnityEngine.Debug.LogError("Please connect a parent to the UILobbyServerList.");
            return;
        }
        online = Parent as UIOnline;
        if (online == null) {
            UnityEngine.Debug.LogError("Please connect a parent of type UIOnline to the UILobbyServerList.");
            return;
        }

        online.client.OnLobbyListRefreshed += OnLobbyListReady;

        UpdateList();
    }

    public override void deactivate()
    {
        base.deactivate();

        if (online.client != null) {
            online.client.OnLobbyListRefreshed -= OnLobbyListReady;
        }

        if (activator) {
            activator.gameObject.SetActive(true);
        }

        // Carefull: SetActive will call OnDisable whict will call deactivate, cousing StackOverflowError.
        this.gameObject.SetActive(false);
    }

    // Called from UX: this is thread safe.

    void CreateLobbyTab(Lobby lobby) {
        var lobbyTab = Instantiate(lobbyTabObject, layout.transform);
        lobbyTab.Parent = this;
        lobbyTab.lobby = lobby;
        lobbyTab.init();

        tabs.Add(lobbyTab);
    }

    public void UpdateList()
    {
        // remove each tabs.
        foreach (var t in tabs) {
            GameObject.Destroy(t.gameObject);
        }
        tabs.Clear();
        // ask server for lobby list.
        online.client.GetLobbies();
    }

    // Need to be thread safe (cf UIOnline)

    public void OnLobbyListReady(SchLobbyClient.RoomListData data) {
        online.pendingCommands.Enqueue(new DisplayRoomListCmd(data.lobbies, this));
    }
}
