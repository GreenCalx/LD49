using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble.UI;
using Schnibble;
using Schnibble.Online;
using Mirror;
using System.Net;
using System.Net.Sockets;
using TMPro;
using Wonkerz;

namespace Wonkerz.UI
{
    public class UILobbyServerTab : UITextTab
    {
        public Lobby lobby;

        public UILabel lobbyName;
        public UILabel lobbyHostName;
        public UILabel lobbyPlayerCount;

        UILobbyServerList serverList;

        override public void Init() {
            base.Init();

            serverList = parent as UILobbyServerList;
            if (serverList == null) {
                this.LogError("Please connect a Parent of type UILobbyServerList to UILobbyServerTab.");
            }

            UpdateView();
        }

        public void UpdateView() {
            lobbyName       .content = lobby.name;
            lobbyHostName   .content = lobby.hostName;
            lobbyPlayerCount.content = (lobby.roomPlayers != null ? lobby.roomPlayers.Count : 0) + " / " + lobby.maxPlayerCount;
        }

        public override void Activate() {
            base.Activate();
            NetworkRoomManagerExt.singleton.JoinLobby(lobby.roomID);
        }

        public void OnClientError(TransportError error, string reason) {
            this.Log("OnClientError.");
            serverList.online.Activate();
        }
    }
}
