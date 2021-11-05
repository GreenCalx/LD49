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
        public bool isFrontAxle;
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
    public AudioRenderer MotorSound;


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
    public float mouseAccumulator = 0;

    public float ButtonForce = -10000;
    private bool WaitForFixedUpdate = false;
    private float CurrentTimeElapsed = 0f;
    public Color MainColor;
    public Color OverchargeColor;
    public GameObject Body;
         public float SpringDistanceMax = 2f;
       public float SpringDistanceMin = 0.1f;
             public float ExpulseForce = 100000;
        public float TimeUntilMax = 0.3f;
        public float TimeAtMaxAvailable = 2f;
        private float TimeAtMaxAvailableElapsed = 0f;
        private bool FilterInputs = false;

    [Header("Tricks")]
    public bool enableTricks;

    private void SetBodyColor(Color C) {
        Body.GetComponent<MeshRenderer>().material.color = C;
    }

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
            GameObject.Destroy(Contact, 2);
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

                if (enableTricks)
                {
                    WheelTrickTracker L_wtt = Axle.LeftWheel.gameObject.GetComponent<WheelTrickTracker>();
                    WheelTrickTracker R_wtt = Axle.RightWheel.gameObject.GetComponent<WheelTrickTracker>();
                    if (L_wtt && R_wtt)
                    {
                        L_wtt.wheel_location = Axle.isFrontAxle ? WHEEL_LOCATION.FRONT_LEFT : WHEEL_LOCATION.BACK_LEFT;
                        R_wtt.wheel_location = Axle.isFrontAxle ? WHEEL_LOCATION.FRONT_RIGHT : WHEEL_LOCATION.BACK_RIGHT;
                    }
                    else
                    {
                        Debug.LogWarning("Trick Mode enabled but missing WheelTrickTracker on wheels.");
                    }
                }
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

        AxleInfos[0].LeftWheel.ConfigureVehicleSubsteps(1, 10, 10);
    }

    void Update()
    {

        var SpringDistance = SpringDistanceMax;

        
        Rigidbody RB = GetComponent<Rigidbody>();

       // new version applies from each wheel apply point the impulse if grounded
       if (Input.GetKey(KeyCode.J)) {

            if (Input.GetKeyDown(KeyCode.J)) {
               FilterInputs = false;
               TimeAtMaxAvailableElapsed = 0;
               CurrentTimeElapsed = 0;
            }
                CurrentTimeElapsed += Time.deltaTime;
                SpringDistance = Mathf.Lerp(SpringDistanceMax, SpringDistanceMin, Mathf.Min(1, CurrentTimeElapsed / TimeUntilMax)) ;
                if (Mathf.Min(1, CurrentTimeElapsed / TimeUntilMax) == 1) {
                    SetBodyColor(OverchargeColor);
                    TimeAtMaxAvailableElapsed += Time.deltaTime;
                    if(TimeAtMaxAvailableElapsed >= TimeAtMaxAvailable && !FilterInputs) {
                        FilterInputs = true;

                            foreach(AxleInfo Axle in AxleInfos) {
                                if (Axle.LeftWheel) {
                                    if (Axle.LeftWheel.isGrounded) {
                                        RB.AddForceAtPosition(RB.transform.up * ExpulseForce/4, Axle.LeftWheel.transform.position, ForceMode.Impulse);
                                    }
                                }
                                if (Axle.RightWheel) {
                                    if (Axle.RightWheel.isGrounded) {
                                        RB.AddForceAtPosition(RB.transform.up * ExpulseForce/4, Axle.RightWheel.transform.position, ForceMode.Impulse);
                                    }
                                }                 
                            }
                            SetBodyColor(MainColor);
                    }
                }
        } else {
            if (!FilterInputs) {
            if (Input.GetKeyUp(KeyCode.J)) {
                foreach(AxleInfo Axle in AxleInfos) {
                    if (Axle.LeftWheel) {
                        if (Axle.LeftWheel.isGrounded) {
                            RB.AddForceAtPosition(RB.transform.up * ExpulseForce/4 * Mathf.Min(1, CurrentTimeElapsed / TimeUntilMax), Axle.LeftWheel.transform.position, ForceMode.Impulse);
                        }
                    }
                    if (Axle.RightWheel) {
                        if (Axle.RightWheel.isGrounded) {
                            RB.AddForceAtPosition(RB.transform.up * ExpulseForce/4 * Mathf.Min(1, CurrentTimeElapsed / TimeUntilMax), Axle.RightWheel.transform.position, ForceMode.Impulse);
                        }
                    }                 
                }
                SetBodyColor(MainColor);
            }
            }

        }



        foreach (AxleInfo Axle in AxleInfos)
        {
            if (Axle.LeftWheel != null)
            {
                var Spring = new JointSpring();
                Spring.spring = this.SuspensionSpring;
                Spring.damper = this.SuspensionDamper;
                Spring.targetPosition = this.TargetPosition;

                Axle.LeftWheel.suspensionSpring = Spring;
                Axle.LeftWheel.suspensionDistance = mouseAccumulator;
            }
            if (Axle.RightWheel != null)
            {
                var Spring = new JointSpring();
                Spring.spring = this.SuspensionSpring;
                Spring.damper = this.SuspensionDamper;
                Spring.targetPosition = this.TargetPosition;

                Axle.RightWheel.suspensionSpring = Spring;
                Axle.RightWheel.suspensionDistance = mouseAccumulator;
            }

        }
    }

    private void SetSpringSize(float SizeDelta)
    {
    }

    public Vector2 MouseLastPosition = Vector2.zero;
    // Update is called once per frame
    void FixedUpdate()
    {


        // Test toffa :
        // the goal is to be able to transform mouse position into
        // spring control. This way we should be able to morph between
        // an arcade type car and monster truck type.
        // Also might be able to jump !
        if (Input.GetMouseButton(0))
        {
            Vector2 MouseDirection = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - MouseLastPosition;
            //if (Mathf.Abs(MouseDirection.y) > 10)
            // Full motion to get to 0 is in percent of screen
            float mouseAccumulatorFactor = 0.5f;
            mouseAccumulator += MouseDirection.y * mouseAccumulatorFactor;
            mouseAccumulator = Mathf.Clamp(mouseAccumulator, 0, 10);
            //else
            //    mouseAccumulator = 0;

            SetSpringSize(mouseAccumulator);
        }
        MouseLastPosition = Input.mousePosition;
        mIsOnGround = false;
        var X = Input.GetAxis("Horizontal");
        var Y = Input.GetAxis("Vertical");

        Rigidbody RB = GetComponent<Rigidbody>();
        BoxCollider BC = GetComponent<BoxCollider>();

        if (Input.GetKeyDown(KeyCode.J)) {
            GetComponent<ConstantForce>().relativeForce = (Vector3.up * -ButtonForce);
        }

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
            else
            {
                axleInfo.LeftWheel.brakeTorque = 0;
                axleInfo.RightWheel.brakeTorque = 0;
                axleInfo.LeftWheel.motorTorque = 0;
                axleInfo.RightWheel.motorTorque = 0;
            }
        }

        WaitForFixedUpdate = false;
    }
}
