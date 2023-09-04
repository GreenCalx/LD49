using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerCarProcAnimator : MonoBehaviour
{
    [Serializable]
    public class PlayerCarProcAnimatorPart
    {
        // object that will be animated, only rendering will be affected
        // we will not change anything physically.
        public GameObject animatee;
        // define how the animator will react to value changes
        public Spring spring;
        // if needed to pump a little the animation
        public float animationMultiplier;
        // 0:x, 1:y, 2:z
        public int axisOfAnimation;
        // max angle of rotation along axis
        public float maxAngle;
        public Vector3 initialRotation;
    }
    public PlayerCarProcAnimatorPart[] parts;
    public Rigidbody car;
    private Vector3 lastCarVelocity;
    private Vector3 carAcceleration;
    // Start is called before the first frame update
    void Start()
    {

        foreach (var p in parts)
        {
            p.initialRotation = p.animatee.transform.localRotation.eulerAngles;
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var p in parts)
        {
            if (p.axisOfAnimation == 1)
            {
                var newPos = Mathf.Clamp01(carAcceleration.z / p.animationMultiplier);
                p.spring.rest = newPos;
                p.animatee.transform.localRotation = Quaternion.Euler(p.initialRotation.x, p.initialRotation.y+Mathf.Clamp01(p.spring.Compute(Time.deltaTime)) * p.maxAngle, p.initialRotation.z);
            }

            if (p.axisOfAnimation == 0)
            {
                var newPos = Mathf.Clamp01(carAcceleration.y / p.animationMultiplier);
                p.spring.rest = newPos;
                p.animatee.transform.localRotation = Quaternion.Euler(p.initialRotation.x+Mathf.Clamp01(p.spring.Compute(Time.deltaTime)) * p.maxAngle,p.initialRotation.y, p.initialRotation.z);
            }

        }
    }

    void FixedUpdate()
    {
        carAcceleration = (lastCarVelocity - car.velocity) / Time.fixedDeltaTime;
        lastCarVelocity = car.velocity;
    }
}
