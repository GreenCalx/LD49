using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    public enum CAM_TYPE {
        UNDEFINED=0,
        HUB=1,
        TRACK=2,
        BOSS=3,
        CINEMATIC=4
    }

    public CAM_TYPE camType = CAM_TYPE.UNDEFINED;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void init() {}
}
