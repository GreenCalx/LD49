using UnityEngine;

public class BridgePhysX : MonoBehaviour
{
    public float OscFrequency = 1;
    public float OscStrength = 30;
    public Vector2 TransStrength;
    private Vector3 StartRot;
    private Vector3 StartTransf;
    private float CurrentTime = 0;
    public Vector3 Velocity;
    private Vector3 LastPosition;
    // Start is called before the first frame update
    void Start()
    {
        StartRot = transform.rotation.eulerAngles;
        StartTransf = transform.position;
        LastPosition = StartTransf;
    }

    void FixedUpdate() {
        var NewPosition = transform.position;
        Velocity = (NewPosition - LastPosition)/Time.deltaTime;
        LastPosition = NewPosition;
    }

    // Update is called once per frame
    void Update()
    {

        CurrentTime += Time.deltaTime;

        var Lerp = Mathf.Sin(CurrentTime / OscFrequency);
        var HalfLerp = Mathf.Cos(CurrentTime / (OscFrequency / 2));

        var Rot = new Quaternion();
        Rot.eulerAngles = new Vector3(StartRot.x + Lerp * OscStrength, StartRot.y, StartRot.z);
        transform.rotation = Rot;

        var Tran = new Vector3(StartTransf.x, StartTransf.y + HalfLerp * TransStrength.x, StartTransf.z - Lerp * TransStrength.y);
        transform.position = Tran;



    }
}
