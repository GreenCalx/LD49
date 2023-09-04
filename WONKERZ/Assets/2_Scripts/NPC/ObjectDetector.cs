using UnityEngine;
using Schnibble;

/**
*   Detects the FIRST SEEN Object carrying 'ObjectDetectable' component
*/
public class ObjectDetector : MonoBehaviour
{
    public Transform detectedTransform;
    public bool objectInRange = false;

    void Start()
    {
        objectInRange = false;
    }

    void Update()
    {
        if (objectInRange && detectedTransform==null)
        { objectInRange = false; }
    }

    void OnTriggerEnter(Collider iCollider)
    {
        ObjectDetectable od = iCollider.GetComponent<ObjectDetectable>();
        if (!!od)
        {
            detectedTransform = od.transform;
            objectInRange = true;
        }
    }

    void OnTriggerExit(Collider iCollider)
    {
        ObjectDetectable od = iCollider.GetComponent<ObjectDetectable>();
        if (!!od && (od.gameObject==transform.gameObject))
        {
            detectedTransform = null;
            objectInRange = false;
        }
    }
}