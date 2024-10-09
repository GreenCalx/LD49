using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// class used to limit FPS to a cerntian number
// For instance while the player is in menus we dant need
// the machine to go full throttle.

public class FPSLimiter : MonoBehaviour
{
    public bool limitFPS = false;
    public int limitValue = 30;


    public void LimitFPS(bool limit) {
        if (limit == limitFPS) return;

        if (limit) {
            if (QualitySettings.vSyncCount == 0) {
                Application.targetFrameRate = limitValue;
            }
        } else {
            Application.targetFrameRate = PlayerPrefs.GetInt("targetFPS");
        }

        limitFPS = limit;
    }
}
