using UnityEngine;

public class testphysx : MonoBehaviour
{
    public Vector3 Velocity;
    private Vector3 LastPosition;
    public Vector3 Accel;
    // Start is called before the first frame update
    void Start()
    {
        LastPosition = transform.position;
    }

    void FixedUpdate()
    {
        transform.position += Mathf.Sin(Time.timeSinceLevelLoad * 0.5f) * new Vector3(1f, 0, 0);
        var NewVelocity = (transform.position - LastPosition)/Time.fixedDeltaTime;
        Accel = (NewVelocity - Velocity);

        Velocity = NewVelocity;
        LastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
