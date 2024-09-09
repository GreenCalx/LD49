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

[System.Serializable]
public class UIConnectionStateText
{
    public enum States
    {
        Connecting,
        Connected,
    };

    float messageDisplayTime = 1.0f;
    float messageDisplayTimeCurrent = 0.0f;

    [SerializeField]
    TextMeshProUGUI text;

    public void SetState(States state)
    {
        switch (state)
        {
            case States.Connecting:
                {
                    text.text = "Connecting...";
                }
                break;
            case States.Connected:
                {
                    text.text = "Connected !";
                }
                break;
        }
    }
};

public class UILobbyServerTab : UITextTab 
{
    public Lobby lobby;

    UIConnectionStateText connectionState;
    UILobbyServerList serverList;

    public class NATPunchClientCmd : UIOnline.UIOnlineMainThreadCommand
    {
        int id;
        IPEndPoint       ep;

        public NATPunchClientCmd(UIOnline uiOnline, int id, IPEndPoint ep):base(uiOnline) {
            this.ep = ep;
            this.id = id;
        }

        public override void Do()
        {
            uiOnline.ConnectToRoom(ep, id);
        }
    }

    override public void init() {
        base.init();

        serverList = Parent as UILobbyServerList;
        if (serverList == null) {
            this.LogError("Please connect a Parent of type UILobbyServerList to UILobbyServerTab.");
        }

        text.text = lobby.name;
    }

    // Force connection to be one second to avoid very fast message blinking.
    float connectionTimerLatency = 1.0f;
    float connectionTimer        = 0.0f;
    public IEnumerator TryConnectToServer() {
        for (int i = 0; i < 10; ++i) {
            if (NetworkClient.active) break;

            this.Log("coroutine tryconnectserver.");
            serverList.online.roomServer.StartClient();
            yield return new WaitForSeconds(0.25f);
        }
    }


    public IEnumerator WaitTimerLatency() {
        while (connectionTimer > 0.0f) {
            connectionTimer -= Time.deltaTime;
            yield return null;
        }
    }

    Queue<MainThreadCommand> pendingCommands = new Queue<MainThreadCommand>();
    override protected void Update()
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

    // Callbacks

    void RegisterCallbacks() {
                                                   NetworkManager.singleton.transport.OnClientConnected += OnClientConnected;
    serverList.online.client.OnStartNATPunch += OnNATPunch;
}

    void RemoveCallbacks() {
        NetworkManager.singleton.transport.OnClientConnected -= OnClientConnected;
        serverList.online.client.OnStartNATPunch -= OnNATPunch;
    }

    public override void activate() {
        base.activate();

        RegisterCallbacks();
        // Try to join lobby => start UX "Connecting..."
        //connectionState.SetState(UIConnectionStateText.States.Connecting);
        // Try to join the lobby.
        serverList.online.client.JoinLobby(lobby.roomID);
    }

    public override void deactivate()
    {
        base.deactivate();

        RemoveCallbacks();
    }

    public void OnClientConnected() {
        StartCoroutine(Coro_OnClientConnected());
    }
    IEnumerator Coro_OnClientConnected() {
        yield return StartCoroutine(WaitTimerLatency());


        deactivate();

        serverList.deactivate();
        serverList.online.deactivate();

        serverList.online.SetState(UIOnline.States.InRoom);
        Access.GameSettings().goToState = UIOnline.States.InRoom;
    }

    public void OnClientError(TransportError error, string reason) {
        this.Log("OnClientError.");
        serverList.online.activate();

        RemoveCallbacks();
    }

    // Need to be on main thread (cf UIOnline)
    public void OnNATPunch(IPEndPoint toNATPunch) {
        pendingCommands.Enqueue(new NATPunchClientCmd(this.serverList.online, this.lobby.hostID, toNATPunch));
    }
}
