using UnityEngine;

using Schnibble;
using Schnibble.UI;

using Wonkerz;

public class UICreateRoomMenu : UIPanelTabbed {
    public UIInputField roomName;

    override public void Activate() {
        base.Activate();

        roomName.Show();
    }

    override public void Deactivate() {
        base.Deactivate();
        roomName.Hide();
    }

    public string GetRoomName() {
        return roomName.text.content;
    }
}
