using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public enum CameraMode
    {
        Follow,
        Fixed,
    };
    [System.Serializable]
    public class CameraDescriptor
    {
        public Vector3 position;
        public CameraMode mode;
        public Quaternion rotation;

    };

    public CameraDescriptor CamDescEnd;
    public CameraDescriptor CamDescStart;
    public Camera Cam;

    public float CameraAnimationLength;
    private float CurrentAnimationLength;
    private bool IsAnimating = false;

    private bool hasManager = false;
    public Transform respawn_location;
    [HideInInspector]
    public CheckPointManager cpm;
    public string name;

    public ParticleSystem particleSystem;
    public ParticleSystemForceField activation_pff;
    public ParticleSystemForceField base_pff;
    // Start is called before the first frame update
    void Start()
    {
        hasManager = subscribeToManager();
        activation_pff.gameObject.active = false;
        base_pff.gameObject.active = true;
    }


    // Update is called once per frame
    void Update()
    {
        if (!hasManager)
            hasManager = subscribeToManager();

        if (IsAnimating)
        {
            CurrentAnimationLength += Time.deltaTime;
            var Lerp = CurrentAnimationLength / CameraAnimationLength;
            if (CurrentAnimationLength < CameraAnimationLength) Cam.transform.position = Vector3.Lerp(CamDescStart.position, CamDescEnd.position, Lerp);
            else IsAnimating = false;
        }

    }

    public bool subscribeToManager()
    {
        GameObject manager = GameObject.Find(Constants.GO_CPManager);
        cpm = manager.GetComponent<CheckPointManager>();
        if (!!cpm)
        {
            cpm.subscribe(this.gameObject);
            return true;
        }
        return false;
    }

    void StartCameraAnimation()
    {
        if (Cam)
        {
            CurrentAnimationLength = 0;
            CamDescStart.position = Cam.transform.position;
            IsAnimating = true;

            //Cam.GetComponent<FollowPlayer>().Active = CamDescEnd.mode == CameraMode.Fixed;
        }
    }

    void OnTriggerEnter(Collider iCol)
    {
        CarController player = iCol.GetComponent<CarController>();
        if (!!player)
        {
            cpm.notifyCP(this.gameObject);
            activation_pff.gameObject.active = true;
            base_pff.gameObject.active = false;

            StartCameraAnimation();
        }
    }

    public GameObject getSpawn()
    {
        return respawn_location.gameObject;
    }

}
