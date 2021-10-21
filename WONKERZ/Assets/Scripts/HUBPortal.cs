using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HUBPortal : MonoBehaviour
{

    public string PORTAL_SCENE_TARGET = "main";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider iCol)
    {
        CarController player = iCol.GetComponent<CarController>();
        if (!!player)
        {
            SceneManager.LoadScene(PORTAL_SCENE_TARGET, LoadSceneMode.Single);
        }
    }
}
