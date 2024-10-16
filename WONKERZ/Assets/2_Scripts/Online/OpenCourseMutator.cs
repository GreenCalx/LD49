using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble.Rendering;

namespace Wonkerz{
    public class OpenCourseMutator : MonoBehaviour
    {
        [Header("Debug Controls")]
        public bool forceNightTime = false;
        public bool forceDayTime = false;

        [Header("Material Collections")]
        public MaterialCollection dayTimeCollection;
        public MaterialCollection nightTimeCollection;

        [Header("Track Mand Refs")]
        public MaterialManager MatMgr;
        [Header("Day/Night")]
        public Light Sun;
        public Material dayTimeSkybox;
        public Color dayTimeFogColor;
        public Light Moon;
        public Material nightTimeSkybox;
        public Color nightTimeFogColor;
        public List<TrackLight> trackLights;
        [Header("Transform Handles")]
        public Transform h_seaTransform;
        

        void Update()
        {
            DebugCalls();
        }

        private void DebugCalls()
        {
            if (forceDayTime)
            {
                DayTime();
                forceDayTime = false;
            }

            if (forceNightTime)
            {
                NightTime();
                forceNightTime = false;
            }
        }


        public void DayTime()
        {
            ToggleTrackLights(false);
            MatMgr.collection = dayTimeCollection;
            
            Sun.gameObject.SetActive(true);
            Moon.gameObject.SetActive(false);

            CameraManager.Instance.changeMainLight(Sun);
            RenderSettings.sun = Sun;
            RenderSettings.skybox = dayTimeSkybox;
            RenderSettings.fogColor = dayTimeFogColor;
        }

        public void NightTime()
        {
            ToggleTrackLights(true);
            MatMgr.collection = nightTimeCollection;

            Sun.gameObject.SetActive(false);
            Moon.gameObject.SetActive(true);

            CameraManager.Instance.changeMainLight(Moon);
            RenderSettings.sun = Moon;
            RenderSettings.skybox = nightTimeSkybox;
            RenderSettings.fogColor = nightTimeFogColor;
        }

        public void ToggleTrackLights(bool iState)
        {
            foreach(TrackLight l in trackLights)
            {
                l.ToggleLight(iState);
            }
        }

    }//!class

}

