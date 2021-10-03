using UnityEngine;

public class WheelRenderer : MonoBehaviour
{
    public GameObject Wheel;
    public float Rotation;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var Collider = Wheel.GetComponent<WheelCollider>();
        // define a hit point for the raycast collision
        // Find the collider's center point, you need to do this because the center of the collider might not actually be
        // the real position if the transform's off.
        var ColliderCenterPoint = Collider.transform.TransformPoint(Collider.center);

        // now cast a ray out from the wheel collider's center the distance of the suspension, if it hit something, then use the "hit"
        // variable's data to find where the wheel hit, if it didn't, then se tthe wheel to be fully extended along the suspension.
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ColliderCenterPoint, -Collider.transform.up, out hit, Collider.suspensionDistance + Collider.radius))
        {
            transform.position = hit.point + (Collider.transform.up * Collider.radius);
        }
        else
        {
            transform.position = ColliderCenterPoint - (Collider.transform.up * Collider.suspensionDistance);
        }

        // now set the wheel rotation to the rotation of the collider combined with a new rotation value. This new value
        // is the rotation around the axle, and the rotation from steering input.
        transform.rotation = Collider.transform.rotation * Quaternion.Euler(Rotation, Collider.steerAngle, 90);
        // increase the rotation value by the rotation speed (in degrees per second)
        Rotation += Collider.rpm * (360 / 60) * Time.deltaTime;

        // define a wheelhit object, this stores all of the data from the wheel collider and will allow us to determine
        // the slip of the tire.
        WheelHit whit = new WheelHit();
        Collider.GetGroundHit(out whit);

        // if the slip of the tire is greater than 2.0, and the slip prefab exists, create an instance of it on the ground at
        // a zero rotation.
        if (Mathf.Abs(whit.sidewaysSlip) > 1.5)
        {
            // if (SlipPrefab)
            // {
            //     Instantiate(SlipPrefab, CorrespondingGroundHit.point, Quaternion.identity);
            // }
        }
    }
}
