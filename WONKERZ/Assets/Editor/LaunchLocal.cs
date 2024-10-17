using System;

using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;

using Schnibble;

namespace Wonkerz
{
    [Serializable]
    public class OnlineTestMenuInspector : EditorWindow
    {
        [SerializeField]
        GameSettings.OnlineTestMenuInspectorData data;
        [SerializeField]
        string clientExePath = "/Build/Win64/Client/Wonkerz.exe";
        [SerializeField]
        string serverExePath = "/Build/Win64/Online/Server/SchLobbyServer.exe";
        [SerializeField]
        string playModeSceneName     = "";
        [SerializeField]
        GameSettings.AutoStartMode externalStartMode;


        [MenuItem("Tools/OnlineTest")]
        private static void Init()
        {
            var window = GetWindow<OnlineTestMenuInspector>("Schnibble Online Test");
            window.Show();
        }

        bool toggleGameParams     = false;
        bool toggleEditorParams   = false;
        bool toggleExternalParams = false;
        void OnGUI() {
            toggleGameParams = EditorGUILayout.BeginFoldoutHeaderGroup(toggleGameParams, "Server game params");
            if (toggleGameParams) {
                data.byPassCourse     = EditorGUILayout.Toggle   ("Bypass course"     , data.byPassCourse    );
                data.byPassTrial      = EditorGUILayout.Toggle   ("Bypass trial"     , data.byPassTrial);
                data.byPassTrialWheel = EditorGUILayout.Toggle   ("Bypass trial wheel", data.byPassTrialWheel);
                data.trialName        = EditorGUILayout.TextField("Trial name:"       , data.trialName       );
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            toggleEditorParams = EditorGUILayout.BeginFoldoutHeaderGroup(toggleEditorParams, "Editor params");
            if (toggleEditorParams) {
                data.autoStartMode    = (GameSettings.AutoStartMode)EditorGUILayout.EnumPopup("AutoStartMode:", data.autoStartMode);
                EditorGUILayout.LabelField("Play mode scene:"  , playModeSceneName);
                if (GUILayout.Button("Choose scene...")) {
                    playModeSceneName = EditorUtility.OpenFilePanel("Choose scene", "Assets", "unity");
                    playModeSceneName = System.IO.Path.GetRelativePath(System.IO.Path.GetDirectoryName(Application.dataPath), playModeSceneName);
                    playModeSceneName = playModeSceneName.Replace(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
                }

                if (AssetDatabase.GetAssetOrScenePath(EditorSceneManager.playModeStartScene) != playModeSceneName) {
                    if (GUILayout.Button("Set start scene")) {
                        SetStartScene();
                    }
                }

                if (EditorSceneManager.playModeStartScene == null) {
                    EditorGUILayout.LabelField("/!\\ No start scene is set! /!\\");
                } else {
                    if (GUILayout.Button("Remove start scene")) {
                        RemoveStartScene();
                    }
                }

            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            toggleExternalParams = EditorGUILayout.BeginFoldoutHeaderGroup(toggleExternalParams, "External params");
            if (toggleExternalParams) {
                clientExePath     = EditorGUILayout.TextField("Exe path:"         , clientExePath        );
                if (GUILayout.Button("Choose path...")) {
                    clientExePath = EditorUtility.OpenFilePanel("Choose client exe", "Build", "exe");
                }
                externalStartMode = (GameSettings.AutoStartMode)EditorGUILayout.EnumPopup("External AutoStartMode:", externalStartMode);
                serverExePath     = EditorGUILayout.TextField("Server exe path:"         , serverExePath        );
                if (GUILayout.Button("Choose path...")) {
                    serverExePath = EditorUtility.OpenFilePanel("Choose server exe", "Build", "exe");
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (GUILayout.Button("Launch editor")) {
                EditorApplication.EnterPlaymode();
            }

            if (GUILayout.Button("Launch external")) {
                LaunchExternal();
            }

            if (GUILayout.Button("Launch headless server")) {
                LaunchServer();
            }
        }

        void SetStartScene() {
            EditorSceneManager.playModeStartScene = (SceneAsset)AssetDatabase.LoadAssetAtPath<SceneAsset>(playModeSceneName);
        }

        void RemoveStartScene() {
            EditorSceneManager.playModeStartScene = null;
        }

        void Update() {
            GameSettings.testMenuData = data;
            GameSettings.testMenuData.autoStartMode = data.autoStartMode;
        }

        void LaunchExternal() {
            var debugModeStr = "";
            switch (externalStartMode) {
                case GameSettings.AutoStartMode.SoloHost: {
                    debugModeStr = "-schDebug_Mode solohost";
                    break;
                }
                case GameSettings.AutoStartMode.Host: {
                    debugModeStr = "-schDebug_Mode host";
                    break;
                }
                case GameSettings.AutoStartMode.Client: {
                    debugModeStr = "-schDebug_Mode client";
                    break;
                }
            }

            var byPassCourse = data.byPassCourse      ? "-byPassCourse" : "";
            var byPassTrial  = data.byPassTrial       ? "-byPassTrial" : "";
            var byPassTrialWheel  = data.byPassTrialWheel  ? "-byPassWheel" : "";
            var trialName    = !string.IsNullOrEmpty(data.trialName) ? "-trialName " + data.trialName : "";

            var arguments    = debugModeStr + " " + byPassCourse + " " + byPassTrial + " " + trialName;

            var path          = clientExePath;
            var clientProcess = System.Diagnostics.Process.Start(path, arguments);
        }

        void LaunchServer() {
            //var path = System.IO.Path.GetDirectoryName(Application.dataPath) + serverExePath;
            var path = serverExePath;
            var serverProcess = System.Diagnostics.Process.Start(path);
        }
    }
}
