using UnityEngine;
using Schnibble;

namespace Wonkerz {
    public class CageKeyCollectible : AbstractCollectible
    {
        public string trackname = "";
        public AudioSource keyCollect_SFX;
        void Start()
        {
            if (Access.managers.collectiblesMgr.hasCageKey(trackname))
            {
                gameObject.SetActive(false);
            }
        }

        protected override void OnCollect()
        {
            Schnibble.Utils.SpawnAudioSource(keyCollect_SFX, transform);
            Access.managers.collectiblesMgr.applyCollectEffect(this);
            Destroy(gameObject);
        }
    }
}
