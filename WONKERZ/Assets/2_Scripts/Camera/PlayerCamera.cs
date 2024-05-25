using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using Schnibble;
namespace Wonkerz
{

    public class PlayerCamera : GameCamera
    {
        [System.Serializable]
        public struct FOVEffect
        {
            [Range(0.01f, 1f)]
            public float thresholdPerCent;
            [Range(0.01f, 10.00f)]
            public float speed;
            [Range(50, 120)]
            public int max;
        }
        public FOVEffect fov;

        public GameObject playerRef;
        [Header("Camera Focus")]
        public float breakFocusDistance = 60f;

        private CameraFocusable loc_secondaryFocus;
        public CameraFocusable secondaryFocus
        {
            get { return loc_secondaryFocus; }
            set
            {
                if (value != loc_secondaryFocus)
                loc_secondaryFocus?.OnPlayerUnfocus();

                value?.OnPlayerFocus();

                loc_secondaryFocus = value;
            }
        }
        public float focusChangeInputLatch = 0.2f;
        public float camDistIncrease = 0f;
        public float camFOVIncrease = 0f;
        protected float elapsedTimeFocusChange = 0f;
        protected bool focusInputLock = false;
        private Queue<CameraFocusable> alreadyFocusedQ;
        protected UISecondaryFocus uISecondaryFocus;



        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            updateSecondaryFocus();
        }

        public void applySpeedEffect(float iSpeedPerCent)
        {
            if (cam == null)
            return;

            float apply_factor = Mathf.Clamp01((Mathf.Abs(iSpeedPerCent) - fov.thresholdPerCent) / fov.thresholdPerCent);

            float nextValue = initial_FOV + initial_FOV * apply_factor;
            float newFOV = Mathf.Clamp(nextValue, initial_FOV, fov.max);

            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, newFOV, Time.deltaTime * fov.speed);
        }


        public override void init() { }

        public override void resetView() { }

        public void SetSecondaryFocus(CameraFocusable iFocusable, bool iShow)
        {
            secondaryFocus = iFocusable;
            showFocus(iShow);
        }

        protected void findFocus()
        {
            showFocus(false);
            alreadyFocusedQ = new Queue<CameraFocusable>();

            PlayerController p = Access.Player();
            Vector3 p_pos = p.GetTransform().position;

            float minDist = float.MaxValue;
            CameraFocusable chosenOne = null;
            List<CameraFocusable> focusables = Access.CameraManager().focusables;
            foreach (CameraFocusable f in focusables)
            {
                if (!f.gameObject.activeSelf)
                continue;

                float dist = Vector3.Distance(f.transform.position, p_pos);
                if (dist > f.focusFindRange)
                continue;

                if (dist < minDist)
                {
                    chosenOne = f;
                    minDist = dist;
                }
            }
            secondaryFocus = chosenOne;

            if (!!secondaryFocus)
            {
                alreadyFocusedQ.Enqueue(secondaryFocus);
                showFocus(true);
            }
        }

        public void resetFocus()
        {
            showFocus(false);
            secondaryFocus = null;
        }

        protected void showFocus(bool iState)
        {
            uISecondaryFocus = Access.UISecondaryFocus();
            if (null == uISecondaryFocus)
            return;

            uISecondaryFocus.gameObject.SetActive(iState);

            if (!!secondaryFocus)
            {
                uISecondaryFocus.trackObjectPosition(secondaryFocus.transform);
                if (iState)
                uISecondaryFocus.setColor(secondaryFocus.focusColor);
            }
            else
            {
                uISecondaryFocus.trackObjectPosition(null);
            }
        }

        protected void updateSecondaryFocus()
        {
            if (null == secondaryFocus)
            {
                return;
            }

            // Check if distance is met, disable otherwise
            PlayerController p = Access.Player();
            Vector3 p_pos = p.GetTransform().position;
            if (Vector3.Distance(secondaryFocus.transform.position, p_pos) > breakFocusDistance)
            {
                resetFocus();
            }
        }

        protected void changeFocus()
        {
            if (null == secondaryFocus)
            { findFocus(); return; }

            this.Log("Change focus");

            PlayerController p = Access.Player();
            Vector3 p_pos = p.GetTransform().position;

            float minDist = float.MaxValue;
            CameraFocusable chosenOne = null;
            List<CameraFocusable> focusables = Access.CameraManager().focusables;

            foreach (CameraFocusable f in focusables)
            {
                if (alreadyFocusedQ.Contains(f))
                continue;

                float dist = Vector3.Distance(f.transform.position, p_pos);
                if (dist > f.focusFindRange)
                continue;

                if (dist < minDist)
                {
                    chosenOne = f;
                    minDist = dist;
                }
            }

            // if every targetable in range were cycle thru
            // then cycle with the queue as long as there is
            // no new ppl in range
            if (chosenOne == null && (alreadyFocusedQ.Count == 0))
            resetFocus();
            else if (chosenOne == null)
            secondaryFocus = alreadyFocusedQ.Dequeue();
            else
            secondaryFocus = chosenOne;

            alreadyFocusedQ.Enqueue(secondaryFocus);
            showFocus(true);
        }

        public void OnFocusRemove(CameraFocusable iFocusable)
        {
            if (secondaryFocus == iFocusable)
            {
                secondaryFocus = null;
                changeFocus();
            }
        }

    }
}
