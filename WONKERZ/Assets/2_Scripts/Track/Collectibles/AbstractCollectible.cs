using UnityEngine;

namespace Wonkerz {
    public abstract class AbstractCollectible : MonoBehaviour
    {
        public enum COLLECTIBLE_TYPE { INFINITE = 0, UNIQUE = 1 }
        public COLLECTIBLE_TYPE collectibleType = COLLECTIBLE_TYPE.INFINITE;

        protected abstract void OnCollect();

        void OnTriggerStay(Collider iCollider)
        {
            if (Utils.isPlayer(iCollider.gameObject))
            {
                OnCollect();
            }
            if (!!iCollider.gameObject.GetComponent<AbstractCollector>())
            {
                OnCollect();
            }
        }

        public void ForceCollect()
        {
            OnCollect();
        }
    }
}
