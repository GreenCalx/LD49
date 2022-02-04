using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPortal : MonoBehaviour
{
    public GameObject playerRef;

    // Start is called before the first frame update
    void Start()
    {
        CarController player = playerRef.GetComponent<CarController>();
        if (!!player)
        {
            playerRef.transform.position = transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
