using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* Handles objects that are not destroyed on scene change and remove duplicates of them
* > To attach on object carrying unique managers that we want to carry from scene to scene
*/
public class ManagerLifecycle : MonoBehaviour
{
    void Awake()
    {
        int n_manager_handler = FindObjectsOfType<ManagerLifecycle>().Length;
        if ( n_manager_handler > 1 )
        {
            Destroy(gameObject);
        } else {
            DontDestroyOnLoad(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
