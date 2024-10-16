using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Wonkerz;

[ExecuteInEditMode]
public class OpenCourseHelper : MonoBehaviour
{
    #if UNITY_EDITOR

    public OpenCourseMutator OCM;
    [Tooltip("Finds all childs with TrackLight script")]
    public bool autoFindTrackLights = false;

    void Update()
    {
        if (Application.isPlaying)
            return;

        if (OCM==null)
            return;

        if (autoFindTrackLights)
        {
            AutoUpdateTrackLightRefs();
            autoFindTrackLights = false;
        }
    }

    public void AutoUpdateTrackLightRefs()
    {
        OCM.trackLights = new List<TrackLight>(GetComponentsInChildren<TrackLight>());
        EditorUtility.SetDirty(OCM);
    }

    #endif
}
