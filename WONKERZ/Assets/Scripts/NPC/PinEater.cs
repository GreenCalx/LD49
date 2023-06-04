using UnityEngine;
using Schnibble;

/**
*   Destroys detectable objects that enters a trigger collider
*/
public class PinEater : MonoBehaviour
{
    void Start()
    { }

    void OnTriggerEnter(Collider iCol)
    {
        if (!!iCol.gameObject.GetComponent<PinBlockade>())
        {
            Destroy(iCol.gameObject);
        }
    }
    void OnTriggerStay(Collider iCol)
    {
        if (!!iCol.gameObject.GetComponent<PinBlockade>())
        {
            Destroy(iCol.gameObject);
        }
    }
}