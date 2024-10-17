using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wonkerz;

[DefaultExecutionOrder(-10000)]
public class CommandLineManager : MonoBehaviour
{
    void Awake() {
        // analyse incoming command line arguments
        string[] commands = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < commands.Length; ++i)
        {
            if (commands[i][0] == '-') {
                if (commands[i] == "-schDebug_Mode") {
                    ProcessDebugMode(commands[++i]);
                }
                else if (commands[i] == "-byPassCourse") {
                    GameSettings.testMenuData.byPassCourse = true;
                } else if (commands[i] == "-byPassTrial") {

                } else if (commands[i] == "-byPassTrialWheel") {
                    GameSettings.testMenuData.byPassTrialWheel = true;
                } else if (commands[i] == "-trialName") {
                    GameSettings.testMenuData.trialName = commands[++i];
                }
            }
        }
    }

    void ProcessDebugMode(string mode) {
        if (mode == "solohost") {
            // launch automatic scene as host.
            GameSettings.testMenuData.autoStartMode = GameSettings.AutoStartMode.SoloHost;
            SceneManager.LoadScene("testLocal");
        } else if (mode == "host") {
            GameSettings.testMenuData.autoStartMode = GameSettings.AutoStartMode.Host;
            SceneManager.LoadScene("testLocal");
        } else if (mode == "client") {
            // launch automatic scene as client, trying to connect to first server available
            GameSettings.testMenuData.autoStartMode = GameSettings.AutoStartMode.Client;
            SceneManager.LoadScene("testLocal");
        }
    }
}
