using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Wonkerz {
    public class PlayerCollisionDetector : MonoBehaviour
    {
        public bool playerInRange = false;
        public Transform player;

        public Transform dummy;
        public bool dummyInRange = false;

        [Header("Optionals")]
        public UnityEvent callBackOnPlayerOnCollision;
        public UnityEvent callBackOnPlayerInCollision;
        public UnityEvent callbackOnPlayerExitCollsion;
    
        // Start is called before the first frame update
        void Start()
        {
            playerInRange = false;
            dummyInRange = false;
            player = null;
            dummy = null;
        }

        void OnCollisionEnter(Collision iCollision)
        {
            if (playerInRange)
            return;

            if (Utils.collisionIsPlayer(iCollision))
            {
                playerInRange = true;
                player = iCollision.collider.transform;
                if (callBackOnPlayerOnCollision!=null)
                callBackOnPlayerOnCollision.Invoke();
                return;
            }

            Dummy d = iCollision.collider.gameObject.GetComponent<Dummy>();
            if (!!d)
            {
                dummyInRange = true;
                playerInRange = true;
                dummy = d.transform;
                player = d.transform;
            }
        }

        void OnCollisionStay(Collision iCollision)
        {
            // If the player dies while in range
            if (Utils.collisionIsPlayer(iCollision))
            {
                if (callBackOnPlayerInCollision!=null)
                callBackOnPlayerInCollision.Invoke();
            }        
        }

        void OnCollisionExit(Collision iCollision)
        {
            if (!playerInRange)
            return;

            if (Utils.collisionIsPlayer(iCollision))
            {
                playerInRange = false;
                player = null;
                if (callbackOnPlayerExitCollsion!=null)
                callbackOnPlayerExitCollsion.Invoke();
            }

            Dummy d = iCollision.collider.gameObject.GetComponent<Dummy>();
            if (!!d)
            {
                dummyInRange = false;
                playerInRange = false;
                dummy = null;
                player = null;
            }        
        }
    }
}
