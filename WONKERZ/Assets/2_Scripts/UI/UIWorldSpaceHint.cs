using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWorldSpaceHint : MonoBehaviour
{
    public bool facePlayer  = false;
    public bool faceCamera  = false;
    public bool yMotionAnim = false;
    public bool scaleAnim   = false;
    
    [Range(0f,1f)]
    public float animYMotionRange = 0f;
    [Range(0f,5f)]
    public float animYMotionSpeed = 0f;

    [Range(0f,1f)]
    public float scaleAnimSize = 1f;
    [Range(0f,5f)]
    public float scaleAnimSpeed = 1f;

    private Transform playerTransform;
    private Vector3 initScale;
    private Vector3 initPosition;

    // Start is called before the first frame update
    void Start()
    {
        playerTransform = Access.Player().transform;
        initScale = transform.localScale;
        initPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (facePlayer)
            transform.LookAt(playerTransform);
        if (faceCamera)
            transform.LookAt(Access.CameraManager().active_camera.transform);

        // scale anim
        if (scaleAnim)
        {
            var value = Mathf.Sin(Time.realtimeSinceStartup * scaleAnimSpeed);
            transform.localScale = initScale + new Vector3(value, value, value) * scaleAnimSize;
        }
        
        // Y pos anim
        if (yMotionAnim)
        {
            var value = Mathf.Sin(Time.realtimeSinceStartup * animYMotionSpeed);
            transform.transform.position = initPosition + new Vector3(0, value, 0) * animYMotionRange;
        }
        
    }
}
