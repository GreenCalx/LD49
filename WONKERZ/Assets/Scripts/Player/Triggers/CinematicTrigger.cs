using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Schnibble;

// Triggers only once in current scene
// > TODO : save seen cinematics to init triggerrs accordingly
public class CinematicTrigger : MonoBehaviour, IControllable
{
    public bool triggerOnlyOnce = true;
    public bool isLevelEntryCinematic = false;
    public bool isSkippable = true;

    private bool triggered = false;

    public bool freezePlayer = true;
    public CinematicCamera cam;

    public CinematicNode rootNode;
    public bool cinematicDone = false;


    // Start is called before the first frame update
    void Start()
    {
        triggered = false;
        cinematicDone = false;
        StartCinematic();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void IControllable.ProcessInputs(InputManager.InputData Entry)
    {
        if (!isSkippable)
            return;

        if (Utils.checkAnyKeyPressed(Entry, false))
            EndCinematic();
    }

    private void EndCinematic()
    {
        Utils.detachControllable<CinematicTrigger>(this);
        cam.gameObject.SetActive(false);
        LevelEntryUI leui = Access.LevelEntryUI();
        if (!!leui)
        {
            leui.gameObject.SetActive(false);
        }
        cam.end();
        if (triggerOnlyOnce)
            Destroy(gameObject);
        cinematicDone = true;
    }

    private void StartCinematic()
    {
        Utils.attachControllable<CinematicTrigger>(this);
        
        triggered = true;
        cam.gameObject.SetActive(true);
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

        // Execute node if defined
        if (rootNode!=null)
        {
            rootNode.execNode();
        }
    }

    void OnTriggerStay(Collider iCollider)
    {
        if (triggerOnlyOnce && triggered)
            return;

        if (Utils.colliderIsPlayer(iCollider))
        {
            StartCinematic();
        }
    }
}
