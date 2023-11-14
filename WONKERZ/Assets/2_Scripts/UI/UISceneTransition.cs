using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISceneTransition : MonoBehaviour
{
    public enum TRANSITION_TYPE {IN, OUT};

    public TRANSITION_TYPE direction;
    public Image self_transiImg;
    public Transform self_start;
    public Transform self_end;
    public float speed = 1f;
    ///
    private float startTime;
    private float journeyLength;

    void Start()
    {
        startTime = Time.time;
        journeyLength = Vector3.Distance(self_start.position, self_end.position);
    }

    void Update()
    {
        float distCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distCovered / journeyLength;

        if (direction==TRANSITION_TYPE.OUT)
            self_transiImg.transform.position = Vector3.Lerp(self_start.position, self_end.position, fractionOfJourney);
        else if (direction==TRANSITION_TYPE.IN)
            self_transiImg.transform.position = Vector3.Lerp(self_end.position, self_start.position, fractionOfJourney);
    }
}
