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
        [Header("Tide Change")]
        public Transform h_seaTransform;
        private Vector3 seaTransformBasePos;
        private Coroutine tideChangeCo;
        
        void Start()
        {
            seaTransformBasePos = h_seaTransform.localPosition;
        }

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

        public void RiseSeaLevel(bool iState, float iTime, float iYAmount)
        {
            if (tideChangeCo!=null)
            {
                StopCoroutine(tideChangeCo);
                tideChangeCo = null;
            }
            if (iState)
                tideChangeCo = StartCoroutine(RiseSea(iTime, iYAmount));
            else
                tideChangeCo = StartCoroutine(LowerSea(iTime));
        }

        IEnumerator RiseSea(float iTime, float iYAmount)
        {
            float elapsedTime = 0f;
            Vector3 targetPos = h_seaTransform.localPosition;
            targetPos.y += iYAmount;

            while (elapsedTime <= iTime)
            {
                h_seaTransform.position  = Vector3.Lerp(seaTransformBasePos, targetPos, elapsedTime/iTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        IEnumerator LowerSea(float iTime)
        {
            float elapsedTime = 0f;
            Vector3 risedPos = h_seaTransform.localPosition;
            while (elapsedTime <= iTime)
            {
                h_seaTransform.position  = Vector3.Lerp(risedPos, seaTransformBasePos, elapsedTime/iTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

    }//!class

}

