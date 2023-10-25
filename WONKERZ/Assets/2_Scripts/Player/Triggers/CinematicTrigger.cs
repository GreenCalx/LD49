using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Schnibble;
using System;

// Triggers only once in current scene
// > TODO : save seen cinematics to init triggerrs accordingly
public class CinematicTrigger : MonoBehaviour, IControllable
{
    [Header("triggerOnceInScene")]
    public bool triggerOnlyOnce = true;
    [Header("triggerOnceInGame")]
    public string uniqueEventID = "";
    [Header("tweaks")]
    public bool isLevelEntryCinematic = false;
    public bool isSkippable = true;
    public float startDelayInSeconds = 0f;
    private float elapsedDelayTime = 0f;
    private bool triggered = false;

    public bool freezePlayer = true;
    public CinematicCamera cam;

    public CinematicNode rootNode;
    public bool cinematicDone = false;

    public float  timeToSkipCinematic = 2f;
    private float elapsedSkipTime = 0f;

    public GameObject skipUIRef;
    private UICinematicSkip skipUIInst;

    private List<InputManager> skipVotes = new List<InputManager>();

    // Start is called before the first frame update
    void Start()
    {
        triggered = false;
        cinematicDone = false;
        skipVotes = new List<InputManager>(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (isSkippable && triggered && !!skipUIInst)
        {
            Debug.Log("skip vote : " + skipVotes.Count);
            if (skipVotes.Count > 0) {
                elapsedSkipTime += Time.deltaTime;
            } else {
                elapsedSkipTime = 0f; Debug.Log("RAZA");
            }
            skipUIInst.updateProgress( elapsedSkipTime / timeToSkipCinematic );

            if (elapsedSkipTime >= timeToSkipCinematic)
            { elapsedSkipTime = 0f; EndCinematic(); }
        }
    }

    void IControllable.ProcessInputs(InputManager currentMgr, GameInput[] Entry)
    {
        if (!isSkippable)
            return;

        if (triggered)
        {
            if ((Entry[(int) PlayerInputs.InputCode.UIValidate] as GameInputButton).GetState().down)
            { 
                if (!skipVotes.Contains(currentMgr))
                    skipVotes.Add(currentMgr);
            }
            else if ((Entry[(int) PlayerInputs.InputCode.UIValidate] as GameInputButton).GetState().up)
            { 
                if (skipVotes.Contains(currentMgr))
                    skipVotes.Remove(currentMgr);
            }
        }
    }

    public void EndCinematic()
    {
        Debug.Log("End Cinematic");

        if (freezePlayer)
        {
            Access.CheckPointManager().player_in_cinematic = false;
            Access.Player().UnFreeze();
        }

        rootNode.forceQuit();

        Access.Player().inputMgr.Detach(this as IControllable);
        
        LevelEntryUI leui = Access.LevelEntryUI();
        if (!!leui)
        {
            leui.gameObject.SetActive(false);
        }
        if (!!cam)
        {
            cam.gameObject.SetActive(false);
            cam.end();
        }

        if (triggerOnlyOnce)
            Destroy(this as CinematicTrigger);
        
        if (!!skipUIInst)
            Destroy(skipUIInst.gameObject);
        
        cinematicDone = true;

        if (!string.IsNullOrEmpty(uniqueEventID))
            Access.GameProgressSaveManager().notifyUniqueEventDone(uniqueEventID);
    }

    public void StartCinematic()
    {
        if (!!triggered)
            return;

        // already triggered at some point in the game
        if (!string.IsNullOrEmpty(uniqueEventID))
        {
            if (Access.GameProgressSaveManager().IsUniqueEventDone(uniqueEventID))
            {
                triggered = true;
                return;
            }
        }

        triggered = true;
        
        if (freezePlayer)
        {
            Access.Player().Freeze();
            Access.CheckPointManager().player_in_cinematic = true;
        }

        Access.Player().inputMgr.Attach(this as IControllable);
        
        if (!!cam)
        {
            cam.gameObject.SetActive(true);
            cam.launch();
        }

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

        if (isSkippable && !!skipUIRef)
        {
            skipUIInst = Instantiate(skipUIRef, transform.parent).GetComponent<UICinematicSkip>();
        }
    }

    void OnTriggerStay(Collider iCollider)
    {
        if (triggerOnlyOnce && triggered)
            return;

        if (Utils.colliderIsPlayer(iCollider))
        {
            if ( elapsedDelayTime <= startDelayInSeconds )
            {
                elapsedDelayTime += Time.deltaTime;
                return;
            }
            StartCinematic();
        }
    }
}
