using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Mirror;

using Schnibble.UI;

public class UIRoom_EmptySlot : UITab
{
    public Image         background;

    // TODO: remove layour ref => should be part of the panel responsibily.
    public UIPanelTabbed onClickPanel;
    public VerticalLayoutGroup tabLayout;

    public UIPanel       invitePlayerPanel;

    public UITextTab defaultTextTab;

    public override void activate()
    {
        base.activate();

        // if (isServver)
        if (NetworkRoomManagerExt.singleton.mode == NetworkManagerMode.Host) {
            // host can invite, remove, etc...
            foreach(var t in onClickPanel.tabs) if (t != null) Destroy(t.gameObject);
            onClickPanel.tabs.Clear();

            var inviteTab = (UITextTab)Instantiate(defaultTextTab, tabLayout.gameObject.transform);
            inviteTab.Parent = this;
            inviteTab.label.text.text = "Invite player";
            inviteTab.onActivate.AddListener(InvitePlayer);
            onClickPanel.tabs.Add(inviteTab);

            var removeSlot = (UITextTab)Instantiate(defaultTextTab, tabLayout.gameObject.transform);
            removeSlot.Parent = this;
            removeSlot.label.text.text = "Remove slot";
            removeSlot.onActivate.AddListener(RemoveSlot);
            onClickPanel.tabs.Add(removeSlot);

            onClickPanel.init();
            onClickPanel.Show();
        }
    }

    void InvitePlayer() {
        // show panel NotImplementedYet
        invitePlayerPanel.Show();
    }

    void RemoveSlot() {
        NetworkRoomManagerExt.singleton.maxConnections -= 1;
    }
}
