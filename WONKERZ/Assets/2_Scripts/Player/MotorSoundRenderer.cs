using UnityEngine;
using Schnibble;

public class MotorSoundRenderer : MonoBehaviour
{
    public SchCar car;

    public float minPitch = 0.8f;
    public float maxPitch = 2.0f;
    public float ratioMul = 1.0f;

    AudioSource source;
    void Awake()
    {
        source = GetComponent<AudioSource>();
    }
    // Update is called once per frame
    void Update()
    {
        // old way
        //GetComponent<AudioSource>().pitch = 0.8f + 1 / ((100) / ((float)car.GetCurrentSpeedInKmH()));

        //new way
        var maxRPM = (float)car.engine.maxRPM;
        var currentRPM = (float)car.engine.rpm;
        var ratio = currentRPM / maxRPM;
        source.pitch = Mathf.Clamp(minPitch + ratioMul * (1 - (1 - ratio)*(1-ratio)), minPitch, maxPitch);
    }
}
