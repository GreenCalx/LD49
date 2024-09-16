using UnityEngine;

using Schnibble;
using Schnibble.UI;

using Wonkerz;

public class UICreateRoomMenu : UIPanelTabbed {
    public UILabel roomName;

    public string GetRoomName() {
        return roomName.text.text;
    }
}
