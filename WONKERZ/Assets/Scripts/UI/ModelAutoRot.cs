using UnityEngine;

public class ModelAutoRot : MonoBehaviour
{
    public float x = 0,
                 y = 0,
                z = 0;
    public float rotSpeed = 250;
    public float rotDamp = 10;

    private float axeRotX, axeRotY, axeRotZ;

    // Start is called before the first frame update
    void Start()
    {
        axeRotX = transform.eulerAngles.x;
        axeRotY = transform.eulerAngles.y;
        axeRotZ = transform.eulerAngles.z;
    }

    // Update is called once per frame
    void Update()
    {
        if (x != 0)
            rotX();
        if (y != 0)
            rotY();
        if (z != 0)
            rotZ();
    }

    private void rotX()
    {
        axeRotX += rotSpeed * Time.unscaledDeltaTime;
        var q = Quaternion.Euler(axeRotX, transform.eulerAngles.y, transform.eulerAngles.z);
        transform.rotation = Quaternion.Lerp(transform.rotation, q, Time.unscaledDeltaTime * rotDamp);
    }

    private void rotY()
    {
        axeRotY += rotSpeed * Time.unscaledDeltaTime;
        var q = Quaternion.Euler(transform.eulerAngles.x, axeRotY, transform.eulerAngles.z);
        transform.rotation = Quaternion.Lerp(transform.rotation, q, Time.unscaledDeltaTime * rotDamp);
    }

    private void rotZ()
    {
        axeRotZ += rotSpeed * Time.unscaledDeltaTime;
        var q = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, axeRotZ);
        transform.rotation = Quaternion.Lerp(transform.rotation, q, Time.unscaledDeltaTime * rotDamp);
    }
}
