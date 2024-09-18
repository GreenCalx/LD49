using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wonkerz {
    public class UIWorldSpaceHint : MonoBehaviour
    {
        public bool isOnline = true;
        public bool facePlayer  = false;
        public bool faceCamera  = false;
        public bool yMotionAnim = false;
        public bool scaleAnim   = false;
    
        [Range(0f,1f)]
        public float animYMotionRange = 0f;
        [Range(0f,5f)]
        public float animYMotionSpeed = 0f;

        [Range(0f,1f)]
        public float scaleAnimSize = 1f;
        [Range(0f,5f)]
        public float scaleAnimSpeed = 1f;

        public float rotationAlongFwdAxis = 0f;

        private Transform playerTransform;
        private Vector3 initScale;
        private Vector3 initPosition;

        private CameraManager CM;

        bool usable = false;

        // Start is called before the first frame update
        void Start()
        {
            if (!isOnline) {
                playerTransform = Access.Player().GetTransform().transform;
            }
            else {
                usable = false;
                StartCoroutine(WaitForPlayer());
                return;
            }
            playerTransform = OnlineGameManager.singleton.localPlayer.self_PlayerController.GetTransform();

            CM = Access.CameraManager();
            initScale = transform.localScale;
            initPosition = transform.position;

            usable = true;
        }

        IEnumerator WaitForPlayer() {
            while (OnlineGameManager.singleton             == null) {yield return null;}
            while (OnlineGameManager.singleton.localPlayer == null) {yield return null;}
            var player = OnlineGameManager.singleton.localPlayer;
            while (player.self_PlayerController        == null) {yield return null; }
            while (player.self_PlayerController.GetTransform() == null) {yield return null; }

            playerTransform = OnlineGameManager.singleton.localPlayer.self_PlayerController.GetTransform();

            CM = Access.CameraManager();
            initScale = transform.localScale;
            initPosition = transform.position;
            usable = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (!usable) return;

            if (facePlayer)
            transform.LookAt(playerTransform);


            if (faceCamera)
            {
                if (CM==null)
                { CM = Access.CameraManager(); }
                else if (!!CM.active_camera)
                { transform.LookAt(CM.active_camera.transform); }
            }

            // scale anim
            if (scaleAnim)
            {
                var value = Mathf.Sin(Time.realtimeSinceStartup * scaleAnimSpeed);
                transform.localScale = initScale + new Vector3(value, value, value) * scaleAnimSize;
            }
        
            // Y pos anim
            if (yMotionAnim)
            {
                var value = Mathf.Sin(Time.realtimeSinceStartup * animYMotionSpeed);
                transform.transform.position = initPosition + new Vector3(0, value, 0) * animYMotionRange;
            }

            if (rotationAlongFwdAxis != 0f)
            {
                float rot_step = rotationAlongFwdAxis * Time.deltaTime;
                transform.RotateAround(transform.position, transform.forward, rot_step);
            }
        
        }
    }
}
