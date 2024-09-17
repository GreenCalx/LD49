using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Wonkerz;

public class Online_SpeedWayFinishLine : MonoBehaviour
{
    public PlayerDetector finishDetector;
    public PlayerDetector halfWayDetector;

    public bool hasPlayerBeenHalfTrack = false;

    public void SetPlayerPassedHalfTrack() {
        hasPlayerBeenHalfTrack = true;
        finishDetector.gameObject.SetActive(true);
    }

    void Start() {
        finishDetector.gameObject.SetActive(false);
        halfWayDetector.gameObject.SetActive(true);
    }
}
