using UnityEngine;

public class CheckPoint : AbstractCameraPoint
{
    [Header("Tweaks")]
    public CollectiblesManager.COLLECT_MOD collectMod;
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

    // Multi checkpoint
    private MultiCheckPoint MCP;


    // Start is called before the first frame update
    void Start()
    {
        if (!!activation_pff)
            activation_pff?.gameObject.SetActive(false);
        if (!!base_pff)
            base_pff?.gameObject.SetActive(true);

        animator = GetComponentInChildren<Animator>();
        if (animator==null)
            animator?.SetBool("TRIGGERED", false);
        MCP = GetComponentInParent<MultiCheckPoint>();
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
        bool alreadyTriggered = (MCP!=null) ? MCP.triggered : false;
        if (!!player && !alreadyTriggered)
        {
            if (!!MCP)
                MCP.triggered = true;

            cpm.notifyCP(this.gameObject, collectMod == CollectiblesManager.COLLECT_MOD.HEAVEN);

            Access.CollectiblesManager().changeCollectMod(collectMod);
            
            if (!!animator)
                animator.SetBool("TRIGGERED", true);

            if (!!activation_pff)
                activation_pff?.gameObject.SetActive(true);
            if (!!base_pff)
                base_pff?.gameObject.SetActive(false);
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
