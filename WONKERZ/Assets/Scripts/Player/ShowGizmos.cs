using UnityEngine;

public class ShowGizmos : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        if (GetComponent<BoxCollider>() != null)
        {
            var Box = GetComponent<BoxCollider>();
            var Size = transform.TransformVector(Box.size);
            var Center = transform.TransformPoint(Box.center);

            Gizmos.DrawCube(Center, Size);
        }

        if (GetComponent<WheelCollider>() != null)
        {
            var Wheel = GetComponent<WheelCollider>();
            var Center = transform.TransformPoint(Wheel.center);
            var Radius = Wheel.radius;

            Gizmos.DrawSphere(Center, Radius);
        }
    }
}
