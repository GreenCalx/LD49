using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        public PlayerController player;

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

        void OnEnable() {
            Access.managers.audioListenerMgr.SetListener(this.gameObject);
        }

        void OnDisable() {
            Access.managers.audioListenerMgr.UnsetListener(this.gameObject);
        }

        // Update is called once per frame
        void Update()
        {
            updateSecondaryFocus();
        }

        public virtual void applySpeedEffect(float iSpeedPerCent)
        {
            if (cam == null)
            return;

            float apply_factor = Mathf.Clamp01((Mathf.Abs(iSpeedPerCent) - fov.thresholdPerCent) / fov.thresholdPerCent);

            float nextValue = initial_FOV + initial_FOV * apply_factor;
            float newFOV = Mathf.Clamp(nextValue, initial_FOV, fov.max);

            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, newFOV, Time.deltaTime * fov.speed);
        }

        public void SetSecondaryFocus(CameraFocusable iFocusable, bool iShow)
        {
            secondaryFocus = iFocusable;
            showFocus(iShow);
        }

        protected bool findFocus()
        {
            showFocus(false);
            alreadyFocusedQ = new Queue<CameraFocusable>();

            PlayerController p = Access.Player();
            Vector3 p_pos = Vector3.zero;
            if (p != null) p_pos = p.GetTransform().position;

            List<CameraFocusable> focusables = new List<CameraFocusable>();

            // find eligibles camfocusables
            int layerMask = (1 << LayerMask.NameToLayer(Constants.LYR_CAMFOCUSABLE));
            Collider[] sphereCastCols = UnityEngine.Physics.OverlapSphere( transform.position, breakFocusDistance, layerMask, QueryTriggerInteraction.Collide);
            for (int i=0;i<sphereCastCols.Length;i++)
            {
                CameraFocusable focusable = sphereCastCols[i].gameObject.GetComponent<CameraFocusable>();
                if (focusable!=null)
                    focusables.Add(focusable);
            }
            if (focusables.Count==0)
            {
                this.Log("No available camera focusables in range.");
                return false;
            }

            // remove camfocusables blocked by environment
            int statEnvLayerMask = (1 << LayerMask.NameToLayer(Constants.LYR_STATICENV));
            int dynEnvLayerMask = (1 << LayerMask.NameToLayer(Constants.LYR_DYNAMICENV));
            int blockingLayerMask = statEnvLayerMask | dynEnvLayerMask;

            List<CameraFocusable> unobstructedFocusables = new List<CameraFocusable>();
            foreach(CameraFocusable f in focusables)
            {
                RaycastHit hit;
                Vector3 rayDir = f.transform.position - transform.position;
                if (UnityEngine.Physics.Raycast(transform.position, rayDir, out hit, Vector3.Distance(transform.position, f.transform.position), blockingLayerMask))
                {
                    //UnityEngine.Debug.DrawRay(transform.position, rayDir, Color.red, 5f);
                    continue;
                }
                //UnityEngine.Debug.DrawRay(transform.position, rayDir, Color.green, 5f);
                unobstructedFocusables.Add(f);
            }


            // select best camerafocusable
            CameraFocusable chosenOne = null;

            unobstructedFocusables.Sort(CompareCamFocusByScore);

            // fdebug
            foreach(CameraFocusable f in unobstructedFocusables)
            { this.Log("CameraFocus " + f.transform.parent.name + " score " + GetCamFocusScore(f)); }

            foreach(CameraFocusable f in unobstructedFocusables)
            {
                if (CamFocusIsVisibleOnScreen(f))
                {
                    chosenOne = f;
                    break;
                }
            }
            secondaryFocus = chosenOne;

            if (!!secondaryFocus)
            {
                alreadyFocusedQ.Enqueue(secondaryFocus);
                showFocus(true);
            }

            return secondaryFocus != null;
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
            Vector3 p_pos = player.GetTransform().position;
            if (Vector3.Distance(secondaryFocus.transform.position, p_pos) > breakFocusDistance)
            {
                resetFocus();
            }
        }

        protected void changeFocus()
        {
            findFocus();
            return;

            // if (null == secondaryFocus)
            // { findFocus(); return; }

            // this.Log("Change focus");

            // Vector3 p_pos = player.GetTransform().position;

            // float minDist = float.MaxValue;
            // CameraFocusable chosenOne = null;
            // List<CameraFocusable> focusables = Access.managers.cameraMgr.focusables;

            // foreach (CameraFocusable f in focusables)
            // {
            //     if (alreadyFocusedQ.Contains(f))
            //     continue;

            //     float dist = Vector3.Distance(f.transform.position, p_pos);
            //     if (dist > f.focusFindRange)
            //     continue;

            //     if (dist < minDist)
            //     {
            //         chosenOne = f;
            //         minDist = dist;
            //     }
            // }

            // // if every targetable in range were cycle thru
            // // then cycle with the queue as long as there is
            // // no new ppl in range
            // if (chosenOne == null && (alreadyFocusedQ.Count == 0))
            // resetFocus();
            // else if (chosenOne == null)
            // secondaryFocus = alreadyFocusedQ.Dequeue();
            // else
            // secondaryFocus = chosenOne;

            // alreadyFocusedQ.Enqueue(secondaryFocus);
            // showFocus(true);

            // elapsedTimeFocusChange = 0.0f;
        }

        // public void OnFocusRemove(CameraFocusable iFocusable)
        // {
        //     if (secondaryFocus == iFocusable)
        //     {
        //         secondaryFocus = null;
        //         changeFocus();
        //     }
        // }

        // ordered from highest to lowest score
        private int CompareCamFocusByScore(CameraFocusable x, CameraFocusable y)
        {
            if (x==null)
            {
                if (y==null)
                    return 0; // eq
                else
                    return 1;
            }
            else
            {
                if (y==null)
                {
                    return -1;
                }
                else
                { // both defined
                    float x_score = GetCamFocusScore(x);
                    float y_score = GetCamFocusScore(y);

                    // return x_score.CompareTo(y_score);
                    if (x_score > y_score)
                        return -1;
                    else if (x_score < y_score)
                        return 1;
                    else
                    return 0;
                }
            }
        }

        private bool CamFocusIsVisibleOnScreen(CameraFocusable iCF)
        {
            Vector3 vpPos = cam.WorldToViewportPoint(iCF.transform.position);
            return (vpPos.x >= 0f && vpPos.x <= 1f && vpPos.y >= 0f && vpPos.y <= 1f && vpPos.z > 0f);
        }


        // score = DotProd + 1/CamDistance
        // MainFactor : The closest DotProduct to 1 the better (aligned with cam fwd thus camfocus aimed by player)
        // Secondary : The smallest dist to the player the better [0f,1f]
        //
        // TODO :   1/ Custom order priority to add to the score : other player > swappable car > crate = jar
        //          2/ We might want to have less Y coordinate impact upon alignement detection with dotprod
        private float GetCamFocusScore(CameraFocusable iCF)
        {
            float dist =  Vector3.Distance(iCF.transform.position ,player.GetTransform().position);
            Vector3 camFwd = player.GetTransform().forward;
            Vector3 relativeFocusPos = iCF.transform.position - player.GetTransform().position;
            float dp = Vector3.Dot(camFwd.normalized, relativeFocusPos.normalized);
            return dp + 1f/dist;
        }
    }
}
