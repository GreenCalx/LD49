using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;
using System.IO;

// Output the build size or a failure depending on BuildPlayer.
public class Builds 
{
    public struct SavedSettings {
        public BuildTarget target;
        public StandaloneBuildSubtarget subTarget;
        public BuildTargetGroup targetGroup;
    }

    public static SavedSettings SaveSettings() {
        SavedSettings save = new SavedSettings();
        save.target    = EditorUserBuildSettings.activeBuildTarget;
        save.subTarget = EditorUserBuildSettings.standaloneBuildSubtarget;
        save.targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        return save;
    }

    public static void RestoreSettings(SavedSettings save) {
        EditorUserBuildSettings.SwitchActiveBuildTarget(save.targetGroup, save.target);
        EditorUserBuildSettings.standaloneBuildSubtarget = save.subTarget;
    }

    public static void CreatePathIfNotExists(string path) {
        var dataPath = Application.dataPath;
        var projectRootFolder = Path.GetDirectoryName(dataPath);
        Debug.Log(projectRootFolder);
        var desiredPath = Path.Combine(projectRootFolder, Path.GetDirectoryName(path));
        if (!Directory.Exists(desiredPath)) {
            Directory.CreateDirectory(desiredPath);
        }
    }

    public static readonly string exeName = "SchLobbyServer";
    public static readonly string scenePath = "Assets/0_Scenes/Online/ServerScene.unity";
    public static void BuildServerPlateform(BuildTarget plateform) {
        var savedSettings = Builds.SaveSettings();

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();

        buildPlayerOptions.scenes = new[] { Builds.scenePath };

        buildPlayerOptions.target = plateform;
        buildPlayerOptions.subtarget        = (int)StandaloneBuildSubtarget.Server;

        string platformName = "";
        string platformExeExtension = "";
        switch (plateform) {
            case BuildTarget.StandaloneLinux64:
                platformName = "Unix64";
                platformExeExtension = "out";
                break;
            case BuildTarget.StandaloneWindows64:
                platformName = "Win64";
                platformExeExtension = "exe";
                break;
            default:
                Debug.LogError("Plateform unknowkn.");
                RestoreSettings(savedSettings);
                return;
        };

        var path = "Build/" + platformName + "/Online/Server/" + Builds.exeName + "." + platformExeExtension;
        Builds.CreatePathIfNotExists(path);
        buildPlayerOptions.locationPathName = path;
        Debug.Log(path);

        buildPlayerOptions.extraScriptingDefines = new string[] { "SCHSERVER" };

        buildPlayerOptions.options = //BuildOptions.EnableHeadlessMode |
                                     // BuildOptions.StripDebugSymbols |
                                     //  BuildOptions.ForceOptimizeScriptCompilation |
                                        BuildOptions.BuildScriptsOnly;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }

        RestoreSettings(savedSettings);
    }

    [MenuItem("Build/BuildServerWin")]
    public static void BuildServerWin()
    {
        Builds.BuildServerPlateform(BuildTarget.StandaloneWindows64);
    }

    [MenuItem("Build/BuildServerUnix")]
    public static void BuildServerLinux()
    {
        Builds.BuildServerPlateform(BuildTarget.StandaloneLinux64);
    }
}
