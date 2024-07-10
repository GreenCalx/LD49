using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;

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


    [MenuItem("Build/BuildServerWin")]
    public static void BuildServerWin()
    {
        var savedSettings = Builds.SaveSettings();

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();

        buildPlayerOptions.scenes = new[] { "Assets/0_Scenes/Online/ServerScene.unity" };

        buildPlayerOptions.locationPathName = "Build/Online/Server/Win64";

        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.subtarget = (int)StandaloneBuildSubtarget.Server;

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
}
