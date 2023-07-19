using UnityEngine;

public class testphysx : MonoBehaviour
{
    public Vector3 Velocity;
    private Vector3 LastPosition;
    public Vector3 Accel;
    public float Speed;
    public float Period;
    // Start is called before the first frame update
    void Start()
    {
        LastPosition = transform.position;
    }

    void FixedUpdate()
    {
        transform.position = LastPosition + Mathf.Sin(Time.timeSinceLevelLoad * Speed) * new Vector3(0, 0, Period);

        var NewVelocity = (transform.position - LastPosition)/Time.fixedDeltaTime;
        Accel = (NewVelocity - Velocity);

        Velocity = NewVelocity;
        //LastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
