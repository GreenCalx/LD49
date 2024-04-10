using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Wonkerz
{

    public class NPCTheKid : NPCDialog
    {
        [Header("#NPCTheKid")]
        public Transform challengeDestination;
        public List<GameObject> tagAsLava;
        public Material LavaMaterial;
        public GameObject bloomPPVolumeHandle;

        public bool challenge_complete = false;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void LaunchFloorIsLava()
        {
            foreach (GameObject go in tagAsLava)
            {
                if (go.GetComponent<LavaTag>() != null)
                continue;

                LavaTag LT = go.AddComponent(typeof(LavaTag)) as LavaTag;
                LT.enable(LavaMaterial);
            }
            bloomPPVolumeHandle.SetActive(true);

            StartCoroutine(PlayerInChallenge());
        }

        public void StopFloorIsLava()
        {
            foreach (GameObject go in tagAsLava)
            {
                LavaTag LT = go.GetComponent<LavaTag>();
                if (LT == null)
                continue;

                LT.disable();
            }
            bloomPPVolumeHandle.SetActive(false);
        }

        IEnumerator PlayerInChallenge()
        {
            PlayerController pc = Access.Player();
            while (pc.IsAlive() && !challenge_complete)
            {
                yield return null;
            }
            StopFloorIsLava();
        }
    }
}
