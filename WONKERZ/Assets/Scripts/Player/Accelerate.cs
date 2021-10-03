using UnityEngine;

public class Accelerate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var P = GameObject.Find("Player");
        var C = P.GetComponentInChildren<CarController>();
        foreach (var Axle in C.AxleInfos)
        {
            Axle.LeftWheel.motorTorque = C.MaxTorque;
            Axle.RightWheel.motorTorque = C.MaxTorque;
        }
    }

    private void Update()
    {
        var P = GameObject.Find("Player");
        var C = P.GetComponentInChildren<CarController>();
        foreach (var Axle in C.AxleInfos)
        {
            Axle.LeftWheel.motorTorque = C.MaxTorque;
            Axle.RightWheel.motorTorque = C.MaxTorque;
        }

    }

}
