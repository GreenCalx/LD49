using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Mirror;

using TMPro;

using Schnibble;
using Schnibble.UI;

public class UIRoom_PlayerSlot : UITab
{
    public Image   background;
    public UILabel latency;
    public UILabel playerName;
    public UILabel readyState;
    public UIElement hostCrown;

    // TODO: remove layour ref => should be part of the panel responsibily.
    public UIPanelTabbed onClickPanel;
    public VerticalLayoutGroup tabLayout;

    public UIPanel       invitePlayerPanel;
    public UITextTab     defaultTextTab;

    public NetworkRoomPlayerExt roomPlayer {get; private set; }
    public void AttachRoomPlayer(NetworkRoomPlayerExt player) {
        if (roomPlayer != null && roomPlayer != player) {
            roomPlayer.onAnyChange -= UpdateView;
        }
        roomPlayer = player;
        if (roomPlayer != null) {
            roomPlayer.onAnyChange += UpdateView;
        }
    }

    public override void Init()
    {
        base.Init();

        hostCrown.Hide();
    }

    public override void Activate()
    {
        base.Activate();

        if (roomPlayer.isLocalPlayer) {
            this.Log("Not implemented for local player, could be used for cosmetics, achievements, etc...");
            return;
        }

        if (NetworkRoomManagerExt.singleton.mode == NetworkManagerMode.Host)
        {
            // host can invite, remove, etc...
            foreach (var t in onClickPanel.tabs) if (t != null) Destroy(t.gameObject);
            onClickPanel.tabs.Clear();

            var inviteTab = (UITextTab)Instantiate(defaultTextTab, tabLayout.gameObject.transform);
            inviteTab.parent = this;
            inviteTab.label.content = "Invite player";
            inviteTab.onActivate.AddListener(InvitePlayer);
            onClickPanel.tabs.Add(inviteTab);

            var removeSlot = (UITextTab)Instantiate(defaultTextTab, tabLayout.gameObject.transform);
            removeSlot.parent = this;
            removeSlot.label.content = "Remove slot";
            removeSlot.onActivate.AddListener(RemovePlayer);
            onClickPanel.tabs.Add(removeSlot);

            onClickPanel.Init();
            onClickPanel.Show();
        } else {
            this.Log("Not implemented for Client only. Could be used for profile display, etc...");
        }
    }

    override protected void OnDestroy() {
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
            // TODO: only react to the change instead of every frame.
            SetReadyState(roomPlayer.readyToBegin);

            // host : can we know without using the diagnostics index?
            if (roomPlayer.index == 0) {
                hostCrown.Show();
            } else {
                hostCrown.Hide();
            }

            if (roomPlayer.isLocalPlayer) {
                (parent as UIRoom).OnLocalPlayerReadyStateChanged(roomPlayer.readyToBegin);
            }
        }

        public void ChangeReadyState(bool value) {
            roomPlayer.CmdChangeReadyState(value);
        }

        public void SetPosition(int x, int y) {
            //this.gameObject.GetComponent<RectTransform>().position = new Vector3(x, y, 0);
            this.gameObject.transform.localPosition = new Vector3(x, y, 0);
        }

        public void SetBackgroundColor(Color c) {
            this.background.color = c;
        }

        public void SetPlayerName(string playerName) {
            this.playerName.content = playerName;
        }

        public void SetReadyState(bool readyState) {
            this.readyState.content = readyState ? "Lock and loaded!" : "Wait for me!";
        }

        public void SetLatency(int latency) {
            this.latency.content = latency.ToString() + " ms";
        }

    public void RemovePlayer() {
        roomPlayer.GetComponent<NetworkIdentity>().connectionToServer.Disconnect();
    }

    void InvitePlayer() {
        // show panel NotImplementedYet
        invitePlayerPanel.Show();
    }
}
