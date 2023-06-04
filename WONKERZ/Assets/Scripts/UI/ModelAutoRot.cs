using UnityEngine;

public class ModelAutoRot : MonoBehaviour
{
    public bool x = false,
                y = false,
                z = false;
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
        // if (!!x)
        //     rotX();
        // if (!!y)
        //     rotY();
        // if (!!z)
        //     rotZ();

        transform.Rotate( 
            ((!!x)?rotSpeed*Time.deltaTime:0f),
            ((!!y)?rotSpeed*Time.deltaTime:0f),
            ((!!z)?rotSpeed*Time.deltaTime:0f)
        );
    }

    private void rotX()
    {
        axeRotX += rotSpeed * Time.deltaTime;
        var q = Quaternion.Euler(axeRotX, transform.eulerAngles.y, transform.eulerAngles.z);
        transform.rotation = Quaternion.Lerp(transform.rotation, q, Time.deltaTime * rotDamp);
    }

    private void rotY()
    {
        axeRotY += rotSpeed * Time.deltaTime;
        var q = Quaternion.Euler(transform.eulerAngles.x, axeRotY, transform.eulerAngles.z);
        transform.rotation = Quaternion.Lerp(transform.rotation, q, Time.deltaTime * rotDamp);
    }

    private void rotZ()
    {
        axeRotZ += rotSpeed * Time.deltaTime;
        var q = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, axeRotZ);
        transform.rotation = Quaternion.Lerp(transform.rotation, q, Time.deltaTime * rotDamp);
    }
}
