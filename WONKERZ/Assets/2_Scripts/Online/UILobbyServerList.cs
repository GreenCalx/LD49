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
    // Externals

    public VerticalLayoutGroup layout;

    public UILobbyServerTab lobbyTabObject;

    public UILabel emptyLabel;

    public UIOnline online { get; private set; }

    public override void Init() {
        base.Init();

        if (parent == null) {
            this.LogError("Please connect a parent to the UILobbyServerList.");
            return;
        }
        online = parent as UIOnline;
        if (online == null) {
            this.LogError("Please connect a parent of type UIOnline to the UILobbyServerList.");
            return;
        }

        // just in case...
        online.client.OnLobbyListRefreshed -= OnLobbyListReady;
        online.client.OnLobbyListRefreshed += OnLobbyListReady;

        UpdateList();
    }

    public override void Deinit() {
        base.Deinit();

        if (online != null && online.client != null) {
            online.client.OnLobbyListRefreshed -= OnLobbyListReady;
        }
    }

    public override void Activate()
    {
        Init();

        base.Activate();

        StartInputs();

    }

    public override void Deactivate()
    {
        base.Deactivate();

        StopInputs();

        if (activator) {
            activator.Show();
        }

        Deinit();
    }

    // Interenals

    Queue<MainThreadCommand> pendingCommands = new Queue<MainThreadCommand>();

    protected override void Update()
    {
        base.Update();

        while (pendingCommands.Count != 0)
        {
            var cmd = pendingCommands.Dequeue();
            if (cmd == null)
            {
                this.LogError("Command is null => very weird!");
            }
            else
            {
                cmd.Do();
            }
        }
    }

    // Called from UX: this is thread safe.

    void CreateLobbyTab(Lobby lobby) {
        var lobbyTab = Instantiate(lobbyTabObject, layout.transform);
        lobbyTab.parent = this;
        lobbyTab.lobby = lobby;
        lobbyTab.Init();

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

    class DisplayRoomListCmd : MainThreadCommand
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
            if (lobbies.Count == 0) {
                ui.emptyLabel.Show();
            } else
            {
                ui.emptyLabel.Hide();

                foreach (var lobby in lobbies)
                {
                    ui.CreateLobbyTab(lobby);
                }

                ui.SelectTab(0);
            }

            OnCmdSuccess?.Invoke();
        }
    };

    void OnLobbyListReady(SchLobbyClient.RoomListData data) {
        pendingCommands.Enqueue(new DisplayRoomListCmd(data.lobbies, this));
    }
}
