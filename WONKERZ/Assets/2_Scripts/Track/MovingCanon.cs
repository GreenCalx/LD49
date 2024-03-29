using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovingCanon : MonoBehaviour
{
    [Header("References")]
    public Canon canon;
    public Transform minPosition;
    public Transform maxPosition;
    [Header("Tweaks")]
    public float speed = 5f;
    public float rateOfFire = 1f;
    public float epsilon = 0.1f;

    [Header("Internals")]
    private float rest_time = 0f;
    private bool isActive = false;
    private Coroutine selfCo;
    private float startTime = 0f;
    private float journeyLength = 0f;

    void Start()
    {
        rest_time = 0f;
        isActive = false;
        transform.position = minPosition.position;
    }

    void Update()
    {
        if (isActive)
        {
            rest_time += Time.deltaTime;
            if (rest_time >= rateOfFire)
            {
                canon.Fire();
                rest_time = 0f;
            }
        }
    }

    public void activate()
    {
        if (isActive)
            return;

        isActive = true;
        startTime = Time.time;
        journeyLength = Vector3.Distance(minPosition.position, maxPosition.position);

        selfCo = StartCoroutine(MoveAndShoot());
    }

    public void deactivate()
    {
        if (!isActive)
            return;
        isActive = false;

        StopCoroutine(selfCo);
    }

    IEnumerator MoveAndShoot()
    {
        bool goToMax = true;
        
        while (isActive)
        {
            float distCovered = (Time.time - startTime) * speed;
            float fractionOfJourney = distCovered / journeyLength;
            if (goToMax)
            {
                float distToMax =  Vector3.Distance(transform.position, maxPosition.position);
                if (distToMax < epsilon)
                {
                    transform.position = maxPosition.position;
                    goToMax = false;
                    startTime = Time.time;
                    yield return null;
                }
                transform.position = Vector3.Lerp(transform.position, maxPosition.position, fractionOfJourney);

            } else { // gotoMin
                float distToMin =  Vector3.Distance(transform.position, minPosition.position);
                if (distToMin < epsilon) 
                {
                    transform.position = minPosition.position;
                    goToMax = true;
                    startTime = Time.time;
                    yield return null;
                }
                transform.position = Vector3.Lerp(transform.position, minPosition.position, fractionOfJourney);
            }
            yield return null;
        }
    }

}