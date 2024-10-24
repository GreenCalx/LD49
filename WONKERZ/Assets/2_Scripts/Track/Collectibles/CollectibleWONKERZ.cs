using UnityEngine;
using System.Collections.Generic;
using System;

namespace Wonkerz {
    public class CollectibleWONKERZ : AbstractCollectible
    {
        public enum LETTERS { W, O, N, K, E, R, Z }
    

        public LETTERS currLetter;
        public float yRotationSpeed = 1f;

        public TrackEvent wonkerzCollectedEvent;
        public AudioSource onCollect_SFX;
        public AudioSource onAllLetters_SFX;

        // Start is called before the first frame update
        void Start()
        {
            collectibleType = COLLECTIBLE_TYPE.UNIQUE;

        }

        // Update is called once per frame
        void Update()
        {
            animate();
        }

        private void animate()
        {
            transform.Rotate(new Vector3(0, yRotationSpeed, 0), Space.World);
        }

        protected override void OnCollect()
        {
            gameObject.SetActive(false);
            //TODO : persist collected status
            Access.managers.collectiblesMgr.applyCollectEffect(this);
            Access.UIWonkerzBar().display();

        
            // check if all letters are collected
            foreach (LETTERS l in Enum.GetValues(typeof(CollectibleWONKERZ.LETTERS)))
            {
                if (!Access.managers.collectiblesMgr.hasWONKERZLetter(l, Access.managers.trackMgr.launchedTrackName))
                {
                    Schnibble.Utils.SpawnAudioSource( onCollect_SFX, transform); // normal SFX
                    return;
                }
            }
            // all letters collected ! special SFX
            Schnibble.Utils.SpawnAudioSource( onAllLetters_SFX, transform);
            wonkerzCollectedEvent.setSolved();
        }
    }
}
