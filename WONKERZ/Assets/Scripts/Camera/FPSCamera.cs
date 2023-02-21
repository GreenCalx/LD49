using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCamera : PlayerCamera
{
    void Awake()
    {
        init();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (playerRef==null)
        {
            Debug.LogWarning("Bad ref on player in FPSCamera.");
            playerRef = Utils.getPlayerRef();
        }
        transform.position = playerRef.transform.position;
        transform.rotation = playerRef.transform.rotation;
    }

    public override void init()
    {
        camType = CAM_TYPE.FPS;
        playerRef = Utils.getPlayerRef();
        transform.position = playerRef.transform.position;
        transform.rotation = playerRef.transform.rotation;
    }
}
