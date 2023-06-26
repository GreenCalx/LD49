using UnityEngine;
using Schnibble;

public class CheckPoint : AbstractCameraPoint
{
    [Header("MAND")]
    public Transform respawn_location;
    public AudioSource  checkpoint_SFX;
    public int id;

    [Header("Tweaks")]
    public string checkpoint_name;
    public CollectiblesManager.COLLECT_MOD collectMod;
    public Camera Cam;

    public float CameraAnimationLength;
    private float CurrentAnimationLength;
    private bool IsAnimating = false;
    private bool alreadyTriggered = false;

    [Header("ANIM")]
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
        if (!!activation_pff)
            activation_pff?.gameObject.SetActive(false);
        if (!!base_pff)
            base_pff?.gameObject.SetActive(true);

        animator = GetComponentInChildren<Animator>();
        if (animator==null)
            animator?.SetBool("TRIGGERED", false);

        if (checkpoint_name=="")
            checkpoint_name = gameObject.name;
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
        if (!Utils.colliderIsPlayer(iCol))
            return;

        if (!alreadyTriggered)
        {
            alreadyTriggered = true;

            cpm.notifyCP(this.gameObject, collectMod == CollectiblesManager.COLLECT_MOD.HEAVEN);

            TrickTracker tt = Access.Player().gameObject.GetComponent<TrickTracker>();
            if (!!tt)
            {
                Access.TrackManager().addToScore(tt.storedScore);
                tt.storedScore = 0;

                tt.trickUI.displayTrackScore(Access.TrackManager().getTrickScore());
                tt.trickUI.displayTricklineScore(0);
            }

            Access.CollectiblesManager().changeCollectMod(collectMod);
            
            if (!!animator)
                animator.SetBool("TRIGGERED", true);
            if (!!checkpoint_SFX)
                checkpoint_SFX.Play();


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
