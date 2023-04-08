using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class CinematicLight : MonoBehaviour
{
    private float internalTimer = 0f;
    private Light light;
    // -------------------------------------
    // Exposed for Cinematic Nodes

    public void turnOff()
    {
        light.enabled = false;
    }

    public void turnOn()
    {
        light.enabled = true;
    }

    public void rampIntensity(float from, float to, float duration)
    {
        internalTimer = 0f;
        light.intensity = from;
        StartCoroutine(intensityFade(from, to, duration));
    }

    // -------------------------------------
    // COROUTINES
    IEnumerator intensityFade(float from, float to, float duration)
    {
        while (internalTimer < duration)
        {
            light.intensity = Mathf.Lerp( from, to, internalTimer/duration);
            internalTimer += Time.deltaTime;
            yield return null;
        }
    }

    // -------------------------------------
    // UNITY
    void Start()
    {
        light = GetComponent<Light>();
    }
}
