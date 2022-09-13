using UnityEngine;

public class CheckPoint : AbstractCameraPoint
{
    [Header("Tweaks")]
    public Camera Cam;

    public float CameraAnimationLength;
    private float CurrentAnimationLength;
    private bool IsAnimating = false;

    [Header("MAND")]
    public Transform respawn_location;
    public Animator animator;
    public ParticleSystem particles_blend;
    public ParticleSystem particles_add;

    [HideInInspector]
    public CheckPointManager cpm;
    public ParticleSystemForceField activation_pff;
    public ParticleSystemForceField base_pff;


    // Start is called before the first frame update
    void Start()
    {
        //hasManager = subscribeToManager();
        activation_pff.gameObject.SetActive(false);
        base_pff.gameObject.SetActive(true);
        if (animator==null)
            animator = GetComponentInChildren<Animator>();
        animator.SetBool("TRIGGERED", false);
    }


    // Update is called once per frame
    void Update()
    {
        if (IsAnimating)
        {
            CurrentAnimationLength += Time.deltaTime;
            var Lerp = CurrentAnimationLength / CameraAnimationLength;
            if (CurrentAnimationLength < CameraAnimationLength) Cam.transform.position = Vector3.Lerp(CamDescStart.position, CamDescEnd.position, Lerp);
            else IsAnimating = false;
        }

    }

    public bool subscribeToManager(CheckPointManager iCPM)
    {
        cpm = iCPM;
        return iCPM.subscribe(this.gameObject);
    }

    void StartCameraAnimation()
    {
        if (Cam)
        {
            CurrentAnimationLength = 0;
            CamDescStart.position = Cam.transform.position;
            IsAnimating = true;
        }
    }

    void OnTriggerEnter(Collider iCol)
    {
        CarController player = iCol.GetComponent<CarController>();
        if (!!player)
        {
            animator.SetBool("TRIGGERED", true);
            cpm.notifyCP(this.gameObject);
            activation_pff.gameObject.SetActive(true);
            base_pff.gameObject.SetActive(false);

            if (!!particles_add)
                particles_add.Stop();
            if (!!particles_blend)
                particles_blend.Stop();

            Cam = Access.CameraManager().active_camera?.GetComponent<Camera>();
            StartCameraAnimation();
        }
    }

    public GameObject getSpawn()
    {
        return respawn_location.gameObject;
    }

}
