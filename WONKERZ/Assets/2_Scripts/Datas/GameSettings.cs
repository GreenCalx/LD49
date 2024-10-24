using UnityEngine;
using Schnibble;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace  Wonkerz
{
    public class GameSettings : MonoBehaviour
    {
        public bool   isLocal           = false;
        public bool   isOnline          = false;
        public string OnlinePlayerAlias = "Player";

        [System.Serializable]
        public struct OnlineTestMenuInspectorData
        {
            public bool byPassCourse;
            public bool byPassTrial;
            public bool byPassTrialWheel;
            public string trialName;
            public AutoStartMode autoStartMode;
        };
        [SerializeField]
        public static OnlineTestMenuInspectorData testMenuData;

        public enum AutoStartMode {
            None,
            SoloHost,
            Host,
            Client,
        };

        public UIThemeSO defaultUITheme;
        public Schnibble.UI.UICancelPanel defaultCancelPanel;

        public void init() {
            this.Log("init.");
            Schnibble.UI.UITheme.defaultUITheme = defaultUITheme.theme;
            Schnibble.UI.UIPanel.defaultCancelPanel = defaultCancelPanel;
        }

        public void Update() {
            while (MainThreadCommand.pendingCommands.Count != 0)
            {
                var cmd = MainThreadCommand.pendingCommands.Dequeue();
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
    }
}
