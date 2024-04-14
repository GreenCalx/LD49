using UnityEngine;

public class ElectricityRay : MonoBehaviour
{
    public Transform From;
    public Transform To;
    public SpringJoint joint;
    private Vector3 initialScale;
    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        transform.position = Vector3.Lerp(From.position, To.position, 0.5f);

        Vector3 relativePos = From.position - To.position;
        transform.rotation = Quaternion.LookRotation(relativePos, Vector3.up);
        transform.Rotate(90f,0f,0f,Space.Self);

        float distance = Vector3.Distance(From.position,To.position);
        transform.localScale = new Vector3(initialScale.x, distance/2f, initialScale.z);
    }
}