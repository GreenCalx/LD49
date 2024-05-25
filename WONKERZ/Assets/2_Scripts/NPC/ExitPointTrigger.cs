using UnityEngine;

namespace Wonkerz
{
    public class ExitPointTrigger : MonoBehaviour
    {
        void OnTriggerStay(Collider iCollider)
        {
            NPC_SQR sqr = iCollider.GetComponent<NPC_SQR>();
            if (!!sqr)
            sqr.exitReached = true;
        }
    }
}
