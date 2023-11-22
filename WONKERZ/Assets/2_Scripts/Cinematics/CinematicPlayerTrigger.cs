using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CinematicPlayerTrigger : MonoBehaviour
{
    
    public UnityEvent callbackOnComplete;
    ///
    [Header("ObjectMovement")]
    private bool isLerping = false;
    public Transform currentDestination;
    public float moveSpeed = 1f;



    void Start()
    {
        isLerping = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentDestination!=null)
        {
            if (!isLerping)
            {
                initLerp();
                StartCoroutine(LerpPosition(currentDestination.position, 5));
            }
        }
    }

    private void initLerp()
    {
        isLerping = true;
        SetKinematic(true);
    }

    private void lerpComplete()
    {
        currentDestination = null;
        if (callbackOnComplete!=null)
            callbackOnComplete.Invoke();
    }

    IEnumerator LerpPosition(Vector3 targetPosition, float duration)
    {
        float time = 0f;

        Vector3 start = transform.position;
        while (time < duration)
        {
            transform.position = Vector3.Lerp(start, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;

        lerpComplete();
    }

    public void GoTo(Transform iDestination)
    {
        isLerping = false;
        currentDestination  = iDestination;
    }

    public void GoToPlayer()
    {
        isLerping = false;
        currentDestination = Access.Player().transform;
    }

    public void SetKinematic(bool iState)
    {
        Rigidbody rb = transform.GetComponent<Rigidbody>();
        if (!!rb)
            rb.isKinematic = iState;

        foreach ( Transform child in transform)
        {
            rb = child.GetComponent<Rigidbody>();
            if (!!rb)
                rb.isKinematic = iState;
        }
    }

}
