using UnityEngine;
using UnityEngine.Events;
using Schnibble;

/**
*   Detects the FIRST SEEN Object carrying 'ObjectDetectable' component
*/
public class ObjectDetector : MonoBehaviour
{
    public Transform detectedTransform;
    public bool objectInRange = false;

    [Header("Optionals")]
    public bool consumeObjectOnTriggEnter = false;
    public bool consumeObjectOnTriggExit = false;
    public UnityEvent callbackOnTriggEnter;

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
        if (objectInRange)
            return;

        ObjectDetectable od = iCollider.GetComponent<ObjectDetectable>();
        if (!!od)
        {
            detectedTransform = od.transform;
            objectInRange = true;

            if (callbackOnTriggEnter!=null)
                callbackOnTriggEnter.Invoke();

            if (consumeObjectOnTriggEnter)
                Destroy(od.gameObject);
        }
    }

    void OnTriggerExit(Collider iCollider)
    {
        if (!objectInRange)
            return;

        ObjectDetectable od = iCollider.GetComponent<ObjectDetectable>();
        if (!!od)
        {
            detectedTransform = null;
            objectInRange = false;

            if (consumeObjectOnTriggExit)
                Destroy(od.gameObject);
        }
    }
}