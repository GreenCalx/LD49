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

        public float distanceFalloff = 0.8f;

        public float force = 10f;
        ///
        private Rigidbody rbToMove;

        // Start is called before the first frame update
        void Start()
        {
            Trigger(true);
            FanAnimator.Play();
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
            // consider to be in physics update.
            Vector3   dir      = transform.right;
            Rigidbody player = Access.Get<PlayerController>().GetRigidbody();
            float     distance = Mathf.Abs(Vector3.Dot(dir, player.position - transform.position));

            float f = force * 1.0f / (distance * distance * distanceFalloff);
            player.AddForce(dir * force, ForceMode.Acceleration);
        }
    }
}
