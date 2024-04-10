using UnityEngine;
using Schnibble;

namespace Wonkerz {
    /**
     *   Destroys detectable objects that enters a trigger collider
     */
    public class PinEater : MonoBehaviour
    {
        void Start()
        { }

        void OnTriggerEnter(Collider iCol)
        {
            tryEat(iCol);
        }
        void OnTriggerStay(Collider iCol)
        {
            tryEat(iCol);
        }

        private void tryEat(Collider iCol)
        {
            PinBlockade pb = iCol.gameObject.GetComponent<PinBlockade>();
            if (!!pb)
            {
                pb.kill();
            }
        }
    }}
