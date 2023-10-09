using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;

public class TutorialBalloon : MonoBehaviour
{
    [Header("Init")]
    public bool enable_move = true;
    public bool disable_balloon_follow = false;
    
    [Header("MAND")]
    public UIGenerativeTextBox display;
    private PlayerController player;
    public Transform self_ScreenForXYRot;
    public Transform self_BallonAttachForYRot;
    public CameraFocusable self_camFocusPoint;

    [Header("TWEAKS")]
    [Range(0f,10f)]
    public float wiggleSize = 0f;
    public float getOutTheWayAngle = 10f;
    public Vector3 axisOfOutOfTheWay ;
    public Vector3 offsetWhenPlayerInAir = new Vector3(0f,5f,0f);
    public Vector3 offsetWhenPlayerGrounded = new Vector3(0f,5f,0f);
    public float extraDistanceToAvoidFuckingPlayer = 10f;

    // ORBIT
    [Header("ORBIT")]
    [Range(0f,2f)]
    public float stayInFrontSpeedFactor = 1.0f;
    [SerializeField] public Transform focus = default;
    [SerializeField, Range(1f, 80f)] public float distance = 5f;
    [SerializeField, Min(0f)] public float focusRadius = 1f;
    [SerializeField, Range(0f, 1f)] public float focusCentering = 0.5f;
    private Vector3 focusPoint, previousFocusPoint;

    public void updateDisplay(List<UIGenerativeTextBox.UIGTBElement> iElems)
    {
        display.elements = iElems;
        display.resetAndGenerate();
    }

    // Start is called before the first frame update
    void Start()
    {
        player = Access.Player();
        
        focus = player.gameObject.transform;
        focusPoint = focus.position;
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
        disable_balloon_follow = false;
    }

    void LateUpdate()
    {
        facePlayer();
        
        if (disable_balloon_follow)
            return;

        if (!enable_move)
        {
            Vector2 v2pos = new Vector2(transform.position.x, transform.position.z);
            Vector2 v2fpos = new Vector2(focus.position.x, focus.position.z);
            enable_move = Vector2.Distance(v2pos, v2fpos) <= distance;
            if (!enable_move)
                return;
        }

        UpdateFocusPoint();
        Quaternion lookRotation;
        lookRotation = transform.localRotation;

        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 lookPosition = focusPoint - lookDirection * distance;
        
       
        //transform.SetPositionAndRotation(lookPosition, lookRotation);

        stayInFrontOfPlayer();
        
        wiggle();
    }

    void UpdateFocusPoint()
    {
        previousFocusPoint = focusPoint;
        Vector3 targetPoint = focus.position;

        if (focusRadius > 0f)
        {
            float distance = Vector3.Distance(targetPoint, focusPoint);
            float t = 1f;
            if (distance > 0.01f && focusCentering > 0f)
            { t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime); }
            if (distance > focusRadius)
            {
                t = Mathf.Min(t, focusRadius / distance);
            }
            focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
        }
        else
        {
            focusPoint = targetPoint;
        }
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

    private void stayInFrontOfPlayer()
    {
        PlayerController pc = Access.Player();
        float playerspeed = pc.car.GetCurrentSpeed();
        Vector3 difference = player.transform.position - transform.position;

        // position
        Vector3 targetPos = player.transform.position + (player.transform.forward * distance);

        if (pc.TouchGround())
            targetPos += offsetWhenPlayerGrounded;
        else
            targetPos += offsetWhenPlayerInAir;
        
        Debug.DrawRay(targetPos, Vector3.forward * 50, Color.red);
        Debug.DrawRay(targetPos, Vector3.up * 50, Color.green);

        //Vector3 outTheWayPos = Quaternion.AngleAxis(getOutTheWayAngle, axisOfOutOfTheWay) * targetPos;
        //transform.position = Vector3.MoveTowards(transform.position, targetPos, playerspeed * stayInFrontSpeedFactor * Time.deltaTime);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, (playerspeed + stayInFrontSpeedFactor) * Time.deltaTime);

    }
}
