using UnityEngine;
using Schnibble;

public class MotorSoundRenderer : MonoBehaviour
{
    public SchCar car;
    // Update is called once per frame
    void Update()
    {
        GetComponent<AudioSource>().pitch = 0.8f + 1 / ((100) / ((float)car.GetCurrentSpeedInKmH()));
    }
}
