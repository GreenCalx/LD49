using Schnibble;
using Schnibble.Managers;
using UnityEngine;

namespace Wonkerz
{
    public class WkzPlayerInputsManager : PlayerInputsManager
    {
        [System.Serializable]
        public struct WonkInputsSaveData
        {
            public SaveDataInput[] inputs;
            // extra data to save.
            public bool inverseCameraMappingX;
            public bool inverseCameraMappingY;
        }

        public struct WonkAllPlayersSaveData
        {
            public WonkInputsSaveData[] playerData;
        }

        // Do not call start or awake, as we will be specifically called by an initializer.
        public override void Start() {}
        public override void Awake() {}

        public override void init()
        {
            this.Log("init");
            base.init();
            if (init_Player1)
            {
                player1.controllers[0] = new GameControllerSlot(GameControllerSlot.Type.Xbox, new PlayerInputs().controller, 0);
            }

            if (init_Player2)
            {
                player2.controllers[0] = new GameControllerSlot(GameControllerSlot.Type.Xbox, new PlayerInputs().controller, 1);
            }

            if (dualPlayers)
            {
                dualPlayers.controllers[0] = new GameControllerSlot(GameControllerSlot.Type.Xbox, new Player0Inputs().controller, 0);
                dualPlayers.controllers[1] = new GameControllerSlot(GameControllerSlot.Type.Xbox, new Player1Inputs().controller, 1);
            }

            if (init_All)
            {
                all.controllers[0] = player1.controllers[0];
                all.controllers[1] = player2.controllers[0];
            }

            Load();
        }

        public override void Save()
        {
            this.Log("Save inputs.");

            WonkAllPlayersSaveData data;
            data.playerData = new WonkInputsSaveData[2];

            if (init_Player1)
            {
                data.playerData[0].inputs = GetPlayerSaveData(player1.controllers[0]).inputs;
                // todo : per player settings.
                data.playerData[0].inverseCameraMappingX = InputSettings.InverseCameraMappingX;
                data.playerData[0].inverseCameraMappingY = InputSettings.InverseCameraMappingY;
            }
            if (init_Player2)
            {
                data.playerData[1].inputs = GetPlayerSaveData(player2.controllers[0]).inputs;
                data.playerData[1].inverseCameraMappingX = InputSettings.InverseCameraMappingX;
                data.playerData[1].inverseCameraMappingY = InputSettings.InverseCameraMappingY;
            }

            var json = JsonUtility.ToJson(data);
            SetSaveFileContent(json);
        }

        public override void Load()
        {
            var json = GetSaveFileContent();
            if (string.IsNullOrEmpty(json))
            {
                this.LogError("Loading failed");
                return;
            };

            var data = JsonUtility.FromJson<WonkAllPlayersSaveData>(json);
            if (data.playerData == null || data.playerData.Length != 2)
            {
                this.LogError("Loading failed : data is corrupted");
                return;
            }

            if (init_Player1)
                SetFromPlayerSaveData(player1.controllers[0], data.playerData[0].inputs);
            if (init_Player2)
                SetFromPlayerSaveData(player2.controllers[0], data.playerData[1].inputs);

            InputSettings.InverseCameraMappingX = data.playerData[0].inverseCameraMappingX;
            InputSettings.InverseCameraMappingY = data.playerData[0].inverseCameraMappingY;
        }
    }
}
