using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitFPS : MonoBehaviour
{
    public int target = 60;
    // Start is called before the first frame update
    void Start()
    {
       QualitySettings.vSyncCount = 0;
       Application.targetFrameRate = target;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
