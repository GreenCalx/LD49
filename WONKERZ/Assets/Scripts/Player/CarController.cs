using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [System.Serializable]
    public class AxleInfo
    {
        public float Width;
        public float Height;
        public float Length;
        public WheelCollider LeftWheel;
        public WheelCollider RightWheel;
        public bool Motor; // is this wheel attached to motor?
        public bool Steering; // does this wheel apply steer angle?
    }

    [Header("Physics")]
    public GameObject CenterOfMass;
    public bool ApplyInstance = true;
    public GameObject WheelColliderInstance;
    public GameObject WheelRenderer;
    public float SpringDamper;
    public float Spring;

    [Header("Direction")]
    public float MaxSteering = 40;
    public float AxleWidth;
    public float AxleHeight;
    public float AxleLength;
    public float TurnRadius;

    [Header("Motor")]
    public float MaxTorque = 10;
    public float MaxBreak = 5;


    [Header("Wheels")]
    public float WheelRadius = 1;
    [Header("Suspension")]
    public float SuspensionSpring;
    public float SuspensionDamper;
    public float TargetPosition;
    public List<AxleInfo> AxleInfos;

    [Header("Internal")]
    [SerializeField]
    private bool mIsOnGround = false;
    public GameObject PhareArriereDroit;
    public GameObject PhareArriereGauche;
    public GameObject FLAMES;


    // TODO toffa : make this better this is a quick fix
    private void OnDestroy()
    {
        if (ApplyInstance)
        {
            foreach (AxleInfo Obj in AxleInfos)
            {
                GameObject.Destroy(Obj.LeftWheel.gameObject);
                Obj.LeftWheel = null;
                GameObject.Destroy(Obj.RightWheel.gameObject);
                Obj.RightWheel = null;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        foreach (var Collision in collision.contacts)
        {
            var Contact = GameObject.Instantiate(FLAMES, Collision.point, Quaternion.identity);
            Contact.SetActive(true);
        }
    }
    // Start is called before the first frame update
    void Awake()
    {
        GetComponent<Rigidbody>().centerOfMass = CenterOfMass.transform.localPosition;
        var CarPosition = this.transform.position;
        foreach (var Axle in AxleInfos)
        {
            if (Axle.Width == 0) Axle.Width = AxleWidth;
            if (Axle.Height == 0) Axle.Height = AxleHeight;
            if (Axle.Length == 0) Axle.Length = AxleLength;

            if (ApplyInstance)
            {
                var tempLeft = GameObject.Instantiate(WheelColliderInstance, gameObject.transform);
                tempLeft.SetActive(true);
                Axle.LeftWheel = tempLeft.GetComponent<WheelCollider>();
                var tempRight = GameObject.Instantiate(WheelColliderInstance, gameObject.transform);
                tempRight.SetActive(true);
                Axle.RightWheel = tempRight.GetComponent<WheelCollider>();
            }
            Axle.LeftWheel.gameObject.transform.localPosition = new Vector3(Axle.Width, -Axle.Height, Axle.Length);
            Axle.RightWheel.gameObject.transform.localPosition = new Vector3(-Axle.Width, -Axle.Height, Axle.Length);

            Axle.LeftWheel.radius = WheelRadius;
            Axle.RightWheel.radius = WheelRadius;

            var LeftRenderer = GameObject.Instantiate(WheelRenderer, gameObject.transform);
            LeftRenderer.SetActive(true);
            LeftRenderer.GetComponent<WheelRenderer>().Wheel = Axle.LeftWheel.gameObject;
            var RightRenderer = GameObject.Instantiate(WheelRenderer, gameObject.transform);
            RightRenderer.SetActive(true);
            RightRenderer.GetComponent<WheelRenderer>().Wheel = Axle.RightWheel.gameObject;

        }
    }

    void Update()
    {
        foreach (AxleInfo Axle in AxleInfos)
        {
            if (Axle.LeftWheel != null)
            {
                var Spring = new JointSpring();
                Spring.spring = this.SuspensionSpring;
                Spring.damper = this.SuspensionDamper;
                Spring.targetPosition = 0.5f;

                Axle.LeftWheel.suspensionSpring = Spring;
            }
            if (Axle.RightWheel != null)
            {
                var Spring = new JointSpring();
                Spring.spring = this.SuspensionSpring;
                Spring.damper = this.SuspensionDamper;
                Spring.targetPosition = 0.5f;

                Axle.RightWheel.suspensionSpring = Spring;
            }

        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        mIsOnGround = false;
        var X = Input.GetAxis("Horizontal");
        var Y = Input.GetAxis("Vertical");

        Rigidbody RB = GetComponent<Rigidbody>();
        BoxCollider BC = GetComponent<BoxCollider>();

        float motor = MaxTorque * Y;
        float steering = MaxSteering * X;

        foreach (AxleInfo axleInfo in AxleInfos)
        {
            if (axleInfo.Steering)
            {
                axleInfo.LeftWheel.steerAngle = steering;
                axleInfo.RightWheel.steerAngle = steering;
            }
            if (axleInfo.Motor)
            {
                if (motor >= 0)
                {
                    PhareArriereDroit.SetActive(false);
                    PhareArriereGauche.SetActive(false);
                    axleInfo.LeftWheel.motorTorque = motor;
                    axleInfo.RightWheel.motorTorque = motor;

                    axleInfo.LeftWheel.brakeTorque = 0;
                    axleInfo.RightWheel.brakeTorque = 0;
                }
                else
                {
                    PhareArriereDroit.SetActive(true);
                    PhareArriereGauche.SetActive(true);
                    axleInfo.LeftWheel.brakeTorque = -motor;
                    axleInfo.RightWheel.brakeTorque = -motor;

                    axleInfo.LeftWheel.motorTorque = 0;
                    axleInfo.RightWheel.motorTorque = 0;
                }
            }
        }
    }
}
