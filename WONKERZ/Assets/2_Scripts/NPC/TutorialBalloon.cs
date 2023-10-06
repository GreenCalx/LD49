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


    [Header("Internals")]

    // ORBIT
    [Header("ORBIT")]
    public float stayInFrontSpeed = 1.0f;
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
    }

    void LateUpdate()
    {
        if (disable_balloon_follow)
            return;

        if (!enable_move)
        {
            enable_move = Vector3.Distance(transform.position, focus.position) <= distance;
            if (!enable_move)
                return;
        }

        UpdateFocusPoint();
        Quaternion lookRotation;
        lookRotation = transform.localRotation;

        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 lookPosition = focusPoint - lookDirection * distance;

       
        //transform.SetPositionAndRotation(lookPosition, lookRotation);

        facePlayer();
        //wiggle();
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

        newPosition.y += Mathf.Sin(Time.realtimeSinceStartup  * 0.5f);

        transform.position = newPosition;
    }

    private void facePlayer()
    {
        PlayerController pc = Access.Player();
        Vector3 difference = player.transform.position - transform.position;

        // position
        Vector3 targetPos = player.transform.position + (player.transform.forward * distance);
        if (pc.TouchGround())
            targetPos.y = 0f;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, stayInFrontSpeed * Time.deltaTime);

        Debug.DrawRay(player.transform.position, player.transform.forward * distance, Color.green);
        Debug.DrawRay(targetPos, Vector3.up * 9999, Color.blue);

        // sub body rots
        float rotationY = Mathf.Atan2(difference.x, difference.z) * Mathf.Rad2Deg;
        float rotationX = Mathf.Atan2(difference.z, difference.y) * Mathf.Rad2Deg - 90f;

        //transform.rotation = Quaternion.Euler(0.0f, rotationY, 0.0f);
        self_BallonAttachForYRot.localRotation = Quaternion.Euler(0f, rotationY, 0f);
        self_ScreenForXYRot.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);

    }
}
