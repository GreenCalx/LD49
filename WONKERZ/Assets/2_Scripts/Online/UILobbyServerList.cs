using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Schnibble;
using Schnibble.UI;
using Schnibble.Online;
using System.Net;
using System.Net.Sockets;

namespace Wonkerz.UI
{

    public class UILobbyServerList : UIPanelTabbedScrollable
    {
        // Externals

        public VerticalLayoutGroup layout;

        public UILobbyServerTab lobbyTabObject;

        public UILabel emptyLabel;

        public UIOnline online { get; private set; }

        void RegisterLobbyCallbacks() {
            RemoveLobbyCallbacks();
            NetworkRoomManagerExt.singleton.lobbyClient.OnLobbyListRefreshed += OnLobbyListReady;
        }

        void RemoveLobbyCallbacks() {
            NetworkRoomManagerExt.singleton.lobbyClient.OnLobbyListRefreshed -= OnLobbyListReady;
        }

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

            RegisterLobbyCallbacks();

            UpdateList();
        }

        public override void Deinit() {
            base.Deinit();

            RemoveLobbyCallbacks();
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
            NetworkRoomManagerExt.singleton.lobbyClient.GetLobbies();
        }

        // Need to be thread safe (cf UIOnline)
        void OnLobbyListReady(SchLobbyClient.RoomListData data) {
            MainThreadCommand.pendingCommands.Enqueue(new MainThreadCommand(
                delegate() {
                    var lobbies = NetworkRoomManagerExt.singleton.lobbyList;
                    if (lobbies.Count == 0) {
                        emptyLabel.Show();
                    } else
                    {
                        emptyLabel.Hide();

                        foreach (var lobby in lobbies)
                        {
                            CreateLobbyTab(lobby);
                        }

                        SelectTab(0);
                    }
                }));
        }
    }
}
