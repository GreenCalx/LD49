using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;

namespace Wonkerz
{
    [DefaultExecutionOrder(-2000)]
    public class WkzGlobalManager : MonoBehaviour
    {

        public static WkzGlobalManager singleton;

        public GameSettings            gameSettings;
        public WkzPlayerInputsManager  playerInputsMgr;
        public SceneLoader             sceneMgr;
        public CameraManager           cameraMgr;
        public CollectiblesManager     collectiblesMgr;
        public TrackManager            trackMgr;
        public BountyArray             bountyArray;
        public GameProgressSaveManager gameProgressSaveMgr;
        public AudioListenerManager    audioListenerMgr;
        public PlayerCosmeticsManager  playerCosmeticsMgr;
        public FPSLimiter              fpsLimiter;

        void LoadPlayerPrefs() {
            string targetFPSStr   = "targetFPS";
            string vsyncStr       = "vsync";


            int targetFPS = PlayerPrefs.GetInt(targetFPSStr);
            int vsync     = PlayerPrefs.GetInt(vsyncStr);

            Application.targetFrameRate = targetFPS;
            QualitySettings.vSyncCount  = vsync;
        }

        void OnDestroy(){
            if (!PlayerPrefs.HasKey("targetFPS")){
                PlayerPrefs.SetInt("targetFPS", Screen.currentResolution.refreshRate);
            }

            PlayerPrefs.SetInt("vsync", QualitySettings.vSyncCount);

            PlayerPrefs.Save();
        }

        void Awake() {
            this.Log("Awake.");

            if (singleton != null) {
                this.LogWarn("WkzGlobalManager already exists : this one will be deleted.");
                DestroyImmediate(this.gameObject);
                return;
            } else {
                singleton       = this;
                Access.managers = this;
                DontDestroyOnLoad(this.gameObject);
            }
            // Init managers that should be there at first frame. IE: titleScene.
            // NOTE: Do not test for null => we want to see the null pointer exception easily.

            // TODO: make a SchnibbleManager class to derive from to make sure there is no Awake,Start, etc.. and only init()

            // Inputs
            if (playerInputsMgr == null) playerInputsMgr = GetComponent<WkzPlayerInputsManager>();
            playerInputsMgr.init();
            playerInputsMgr.Load();
            // Audio
            if (audioListenerMgr == null) audioListenerMgr = GetComponent<AudioListenerManager>();
            audioListenerMgr.init();
            // Camera
            if (cameraMgr == null) cameraMgr = GetComponent<CameraManager>();
            cameraMgr.init();
            // Gameplay
            if (gameProgressSaveMgr == null) gameProgressSaveMgr = GetComponent<GameProgressSaveManager>();
            gameProgressSaveMgr.init();

            if (gameSettings == null) gameSettings = GetComponent<GameSettings>();
            gameSettings.init();

            if (playerCosmeticsMgr == null) playerCosmeticsMgr = GetComponent<PlayerCosmeticsManager>();
            playerCosmeticsMgr.init();

            if (sceneMgr == null) sceneMgr = GetComponent<SceneLoader>();
            sceneMgr.init();

            if (trackMgr ==null) trackMgr = GetComponent<TrackManager>();

            if (bountyArray ==null) bountyArray = GetComponent<BountyArray>();

            if (fpsLimiter == null) fpsLimiter = GetComponent<FPSLimiter>();

            this.Log("init done.");
        }
    }
}
