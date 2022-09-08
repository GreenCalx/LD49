using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HUBPortal : MonoBehaviour
{
    public string PORTAL_SCENE_TARGET = "main";

    private bool is_loading = false;

    // Start is called before the first frame update
    void Start()
    {
        is_loading = false;
    }

    void OnTriggerStay(Collider iCol)
    {
        CarController player = iCol.GetComponent<CarController>();
        if (!!player && !is_loading)
        {
            is_loading = true;
            Access.SceneLoader().loadScene(PORTAL_SCENE_TARGET);
        }
    }

}
