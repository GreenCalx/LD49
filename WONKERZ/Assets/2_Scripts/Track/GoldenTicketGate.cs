using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using Schnibble;


namespace Wonkerz
{
    public class GoldenTicketGate : MonoBehaviour
    {
        public Transform self_gateRef;
        public Transform gateOpened;
        public Transform gateClosed;

        public int goldenTicketNeeded;
        public PlayerDetector playerDetector;

        public float gateOpenTime = 1f;
        [Header("Internals")]
        public bool isOpened = false;

        void Start()
        {
            if (!isOpened)
            {
                StartCoroutine(closeGate(0f));
            }
            else
            {
                StartCoroutine(openGate(0f));
            }
        }

        public void tryOpenGate()
        {
            if (isOpened)
                return;

            if (Access.CollectiblesManager().getCollectedTickets() >= goldenTicketNeeded)
            {
                StartCoroutine(openGate(gateOpenTime));
                isOpened = true;
            }

        }

        IEnumerator openGate(float iOpenTime)
        {
            float elapsedTime = 0f;
            Vector3 dest = gateOpened.position;
            while (elapsedTime < iOpenTime)
            {
                self_gateRef.position = Vector3.Lerp(self_gateRef.position, dest, (elapsedTime / iOpenTime));
                elapsedTime += Time.deltaTime;

                // Yield here
                yield return null;
            }
            self_gateRef.position = dest;
            yield return null;
        }

        IEnumerator closeGate(float iCloseTime)
        {
            float elapsedTime = 0f;
            Vector3 dest = gateClosed.position;
            while (elapsedTime < iCloseTime)
            {
                self_gateRef.position = Vector3.Lerp(self_gateRef.position, dest, (elapsedTime / iCloseTime));
                elapsedTime += Time.deltaTime;

                // Yield here
                yield return null;
            }
            self_gateRef.position = dest;
            yield return null;
        }

    }
}
