using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Schnibble;

using Wonkerz;

public class SpeedLinesEffect : MonoBehaviour
{
    public ParticleSystem effect;
    public WkzCar         car;

    private void Apply() {
        var effectRatio = car.GetSpeedEffectRatio();
        if (effect)
        {
            var e = effect.emission;
            e.enabled = effectRatio != 0.0f;

            var rb = car.chassis.GetBody();
            if (rb == null) return;

            var relativeWindDir = rb.velocity;
            if (rb.isKinematic) {
                // try to get velocity by another mean for online.
                if (rb.gameObject.TryGetComponent<SchRigidbodyBehaviour>(out SchRigidbodyBehaviour schRB)) {
                    relativeWindDir = schRB.GetVelocity();
                }
            }

            effect.transform.LookAt(rb.gameObject.transform.position + relativeWindDir);

            var lifemin = 0.2f;
            var lifemax = 0.6f;
            var partmain = effect.main;
            partmain.startLifetime = Mathf.Lerp(lifemin, lifemax, effectRatio);
        }
    }

    void Update() {
        Apply();
    }
}
