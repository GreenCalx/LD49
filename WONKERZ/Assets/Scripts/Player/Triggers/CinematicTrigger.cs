using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Triggers only once in current scene
// > TODO : save seen cinematics to init triggerrs accordingly
public class CinematicTrigger : MonoBehaviour, IControllable
{

    public bool triggerOnlyOnce = true;
    public bool isLevelEntryCinematic = false;
    public bool isSkippable = true;

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

    void IControllable.ProcessInputs(InputManager.InputData Entry) 
    {
        if (!isSkippable)
            return;

        if (Entry.Inputs["Jump"].Down)
            EndCinematic();
    }

    void OnDestroy()
    {
        if (!triggered)
            EndCinematic();
    }

    private void EndCinematic()
    {
        Utils.detachControllable<CinematicTrigger>(this);

        LevelEntryUI leui = Access.LevelEntryUI();
        if (!!leui)
        {
            leui.gameObject.SetActive(false);
            cam.end();
            if (triggerOnlyOnce)
                Destroy(gameObject);
        }
    }

    private void StartCinematic()
    {
        Utils.attachControllable<CinematicTrigger>(this);

        triggered = true;
        cam.launch();
        
        // if is a level entry cinematic, display the right UI
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

    // TODO : temp solution while there is no callback
    void OnTriggerExit(Collider iCollider)
    {
        if (!triggered)
            return;
            
        if (Utils.isPlayer(iCollider.gameObject))
        {
            EndCinematic();
        }
    }

    void OnTriggerStay(Collider iCollider)
    {
        if (triggerOnlyOnce && triggered)
            return;

        if (!!iCollider.GetComponent<CarController>())
        {
            StartCinematic();
        }
    }
}
