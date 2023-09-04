using UnityEngine;
using Schnibble;

public class Tornado : MonoBehaviour
{
    public float Force;
    // Start is called before the first frame update
    void Start()
    {

    }

    void OnTriggerEnter(Collider C)
    {
        CarController CC = C.gameObject.GetComponent<CarController>();
        if (CC)
        {
            // apply upward force
            var RB = CC.GetComponent<Rigidbody>();
            RB.AddForceAtPosition(-transform.up * Force, CC.centerOfMass.transform.position, ForceMode.VelocityChange);
        }
    }

    private void OnTriggerExit(Collider C)
    {
        CarController CC = C.gameObject.GetComponent<CarController>();
        if (CC)
        {
            //CC.SetMode(CarController.CarMode.DELTA);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
