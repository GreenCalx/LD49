using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wonkerz {
    public class FanTrap : MonoBehaviour
    {
        public bool isTriggered = false;
        public ParticleSystem WindPS_Ref;
        public GameObject WindCollider;
        public Animation FanAnimator;

        public float force = 10f;
        ///
        private Rigidbody rbToMove;

        // Start is called before the first frame update
        void Start()
        {
            Trigger(true);
            FanAnimator.Play();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void updateAnimSpeed(float iSpeed)
        {
            foreach (AnimationState state in FanAnimator)
            {
                state.speed = iSpeed;
            }
        }

        public void Trigger(bool iState)
        {
            if (iState)
            {
                WindPS_Ref.Play();
                WindCollider.SetActive(true);
                updateAnimSpeed(1f);
            } else {
                WindPS_Ref.Stop();
                WindCollider.SetActive(false);
                updateAnimSpeed(0.1f);
            }
            isTriggered = iState;
        }

        public void PushBackPlayer()
        {
            PlayerDetector pd = WindCollider.GetComponent<PlayerDetector>();
            Vector3 dir = transform.right;
            Access.Get<PlayerController>().GetRigidbody().AddForce( dir * force * Time.deltaTime, ForceMode.VelocityChange );
        }
    }
}
