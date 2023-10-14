using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Schnibble;

public class TutorialBalloon : MonoBehaviour
{
    public enum BALLOON_XANGLE { LEFT = 0, MIDDLE = 1, RIGHT = 2};
    public enum BALLOON_YANGLE { UP = 0, MIDDLE = 1, DOWN = 2};
    [Header("Init")]
    public bool enable_move = true;
    public bool disable_balloon_follow = false;
    
    [Header("MAND")]
    public UIGenerativeTextBox display;
    private PlayerController player;
    public Transform self_ScreenForXYRot;
    public Transform self_BallonAttachForYRot;
    public CameraFocusable self_camFocusPoint;
    public ConfigurableJoint self_jointToPlayer;
    public TutorialPlayerAnchorPoint PlayerAnchorPoint;

    [Header("TWEAKS")]
    [Range(0f,10f)]
    public float wiggleSize = 0f;
    public float maxSpeed = 40f;
    public float xangle = 10f;
    public float yangle = 10f;
    [SerializeField, Range(1f, 80f)] public float distance = 5f;

    [Header("Internals")]
    public Rigidbody rb;
    private TutorialBalloonTrigger _currTrigger;
    public TutorialBalloonTrigger currTrigger
    {
        set
        {
            if (_currTrigger!=null)
            { Access.PlayerInputsManager().player1.Attach(_currTrigger as IControllable);}
            _currTrigger = value;
            if(_currTrigger!=null)
                Access.PlayerInputsManager().player1.Detach(_currTrigger as IControllable);
        }

        get { return _currTrigger; }
    }

    public void updateDisplay(List<UIGenerativeTextBox.UIGTBElement> iElems)
    {
        display.elements = iElems;
        display.resetAndGenerate();
    }

    // Start is called before the first frame update
    void Start()
    {
        player = Access.Player();
        rb = GetComponent<Rigidbody>();
        
        //facePlayer();
        StartCoroutine(launchOnFocused());
        disable_balloon_follow = true;
    }

    IEnumerator launchOnFocused()
    {
        PlayerCamera pc = (PlayerCamera)Access.CameraManager().active_camera;
        if (null==pc)
        {
            Debug.LogError("Wrong type of camera to launch tutorial balloon. Need a player camera.");
            yield break;
        }
        while (pc.secondaryFocus != self_camFocusPoint)
        {
            yield return null;
        }
        // Focus & activated !
        
        // joint update
        //self_jointToPlayer.connectedBody = Access.Player().car.rb;
        PlayerAnchorPoint.trigger();
        self_jointToPlayer.connectedBody = PlayerAnchorPoint.GetComponent<Rigidbody>();
        self_jointToPlayer.autoConfigureConnectedAnchor = false;
        updateAnchor(BALLOON_XANGLE.MIDDLE, BALLOON_YANGLE.UP);

        disable_balloon_follow = false;
    }

    public void disable()
    {
        if (null!=PlayerAnchorPoint)
            PlayerAnchorPoint.delete();
        self_jointToPlayer.connectedBody = null;
        self_jointToPlayer.autoConfigureConnectedAnchor = true;
        enable_move = false;
        disable_balloon_follow = true;
    }

    public void updateAnchor(BALLOON_XANGLE iXAngle, BALLOON_YANGLE iYAngle)
    {
        float loc_xangle = 0f;
        if (iXAngle == BALLOON_XANGLE.LEFT)
        { loc_xangle = (-1) * xangle; }
        else if (iXAngle == BALLOON_XANGLE.RIGHT)
        { loc_xangle = xangle; }

        float loc_yangle = 0f;
        if (iYAngle == BALLOON_YANGLE.DOWN)
        { loc_yangle = (-1) * yangle; }
        else if (iYAngle == BALLOON_YANGLE.UP)
        { loc_yangle = yangle; }

        self_jointToPlayer.connectedAnchor = new Vector3(loc_xangle, loc_yangle, distance);
    }

    void FixedUpdate()
    {
        if (!!rb)
        {
            if (rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }
        }
    }

    void LateUpdate()
    {
        facePlayer();
        
        //wiggle();
    }

    public void wiggle()
    {
        Vector3 newPosition = transform.position;

        newPosition.y += Mathf.Sin(Time.realtimeSinceStartup  * wiggleSize);

        transform.position = newPosition;
    }

    private void facePlayer()
    {
        PlayerController pc = Access.Player();
        Vector3 difference = player.transform.position - transform.position;

        // position
        Vector3 targetPos = player.transform.position + (player.transform.forward * distance);
        //if (pc.TouchGround())
            targetPos.y = 0f;
        // sub body rots
        float rotationY = Mathf.Atan2(difference.x, difference.z) * Mathf.Rad2Deg + 90f;
        //float rotationX = Mathf.Atan2(difference.z, difference.y) * Mathf.Rad2Deg - 90f;
        Vector3 look = Quaternion.LookRotation(difference).eulerAngles;

        //transform.rotation = Quaternion.Euler(0.0f, rotationY, 0.0f);
        self_BallonAttachForYRot.localRotation = Quaternion.Euler(0f, rotationY, 0f);
        self_ScreenForXYRot.localRotation = Quaternion.Euler(look.x, rotationY, 0f);
    }
}
