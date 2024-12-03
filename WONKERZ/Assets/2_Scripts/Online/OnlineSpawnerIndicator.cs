using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineSpawnerIndicator : MonoBehaviour
{
    public List<Transform> orderedTimerLights;
    private int lightIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        lightIndex = 0;
    }

    public void updateTimerLights()
    {
        
    }
}
