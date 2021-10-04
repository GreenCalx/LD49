using UnityEngine;

public class BridgePhysX : MonoBehaviour
{
    public float OscFrequency = 1;
    public float OscStrength = 30;
    public float TransStrength = 10;
    private Vector3 StartRot;
    private Vector3 StartTransf;
    private float CurrentTime = 0;
    // Start is called before the first frame update
    void Start()
    {
        StartRot = transform.rotation.eulerAngles;
        StartTransf = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        CurrentTime += Time.deltaTime;

        var Lerp = Mathf.Sin(CurrentTime / OscFrequency);
        var HalfLerp = Mathf.Sin(CurrentTime / (OscFrequency / 2));

        var Rot = new Quaternion();
        Rot.eulerAngles = new Vector3(StartRot.x + Lerp * OscStrength, StartRot.y, StartRot.z);
        transform.rotation = Rot;

        var Tran = new Vector3(StartTransf.x, StartTransf.y, StartTransf.z - Lerp * TransStrength);
        transform.position = Tran;

    }
}
