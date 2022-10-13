using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiCheckPoint : MonoBehaviour
{
    public CheckPoint[] childs;
    public bool triggered;

    // Start is called before the first frame update
    void Start()
    {
        childs = GetComponentsInChildren<CheckPoint>();
        triggered = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
