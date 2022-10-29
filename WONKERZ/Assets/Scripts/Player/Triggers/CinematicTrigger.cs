using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Triggers only once in current scene
// > TODO : save seen cinematics to init triggerrs accordingly
public class CinematicTrigger : MonoBehaviour
{

    public bool triggerOnlyOnce = true;
    public bool isLevelEntryCinematic = false;

    private bool triggered= false;

    public bool freezePlayer = true;
    public CinematicCamera cam;
    // Start is called before the first frame update
    void Start()
    {
        triggered =false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCinematicEnd()
    {
        if (!isLevelEntryCinematic)
            return;

        LevelEntryUI leui = Access.LevelEntryUI();
        if (!!leui)
        {
            leui.gameObject.SetActive(true);
        }
    }

    // TODO : temp solution while there is no callback
    void OnTriggerExit(Collider iCollider)
    {
        if (!triggered)
            return;
            
        if (Utils.isPlayer(iCollider.gameObject))
        {
            if (isLevelEntryCinematic)
            {
                LevelEntryUI leui = Access.LevelEntryUI();
                if (!!leui)
                {
                    leui.gameObject.SetActive(false);
                }
            }
        }
    }

    void OnTriggerStay(Collider iCollider)
    {
        if (triggerOnlyOnce && triggered)
            return;

        if (!!iCollider.GetComponent<CarController>())
        {
            triggered = true;
            cam.launch();
            // do cinematic stuff    
            if (isLevelEntryCinematic)
            {
                // display UI
                LevelEntryUI leui = Access.LevelEntryUI();
                if (!!leui)
                {
                    // TODO : Callback to deactivate
                    leui.gameObject.SetActive(true);
                }
            }
        }
        
    }
}
