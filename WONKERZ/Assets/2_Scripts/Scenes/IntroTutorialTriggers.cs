using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wonkerz {

// Not MVC cause as this is only use for the intro scene, integrating 
public class IntroTutorialTriggers : MonoBehaviour
{
    public UITutorialStopWindow tutoWindow_Inst;

    public bool firstCheckPointDone = false;
    public UITutorialStopWindow firstCheckPointWindow;

    public bool firstNutTutoDone = false;
    public UITutorialStopWindow firstNutTutoWindow;

    public bool weightTutoDone = false;
    public UITutorialStopWindow weightTutoWindow;

    public bool panelTutoDone = false;
    public UITutorialStopWindow panelTutoWindow;

    public bool getUpTutoDone = false;
    public UITutorialStopWindow getUpTutoWindow;
    public float timeThrStuckDetection = 1f;

    private CollectiblesManager CM;
    private PlayerController PC;
    private CheckPointManager CPM;

    private float elapsedTimeStuckDetection = 0f;

    // Start is called before the first frame update
    void Start()
    {
        CM = Access.CollectiblesManager();
        PC = Access.Player();
        CPM = Access.CheckPointManager();

        elapsedTimeStuckDetection = 0f;
        tutoWindow_Inst = null;
    }

    // Update is called once per frame
    void Update()
    {
        pollFirstCheckpoint();
        pollFirstNut();
        pollPlayerStuck();
        pollPlayerRespawned();
    }

    public void pollFirstCheckpoint()
    {
        if (firstCheckPointDone)
        return;

        if (CPM.last_checkpoint != CPM.race_start)
        {
            SpawnWindow(firstCheckPointWindow);
            firstCheckPointDone = true;
        }
    }

    public void pollPlayerRespawned()
    {
        if (panelTutoDone)
        return;

        if (!firstCheckPointDone)
        return;

        // this.Log("P STATE : " + PC.generalStates.GetState().name);
        // if (PC.generalStates.GetState().name == "Dead")
        if (CPM.saveStateLoaded)
        {
            SpawnWindow(panelTutoWindow);
            panelTutoDone = true;
        }
    }

    public void pollPlayerStuck()
    {
        if (getUpTutoDone)
        return;
        
        bool playerIsActive = PC.IsInMenu();
        bool playerIsGrounded = PC.car.GetCar().IsTouchingGroundAllWheels();

        //bool playerHasLowSpeed = (PC.car_old ? PC.car_old.GetCurrentSpeed() : PC.car_new.GetCurrentSpeed()) < 5f;
        bool playerHasLowSpeed = true;
        

        if (!playerIsActive && !playerIsGrounded && playerHasLowSpeed)
        {
            elapsedTimeStuckDetection += Time.deltaTime;
            if (elapsedTimeStuckDetection >= timeThrStuckDetection)
            {
                SpawnWindow(getUpTutoWindow);
                getUpTutoDone = true; 
            }
        } else {
            elapsedTimeStuckDetection = 0f;
        }
    }

    public void pollFirstNut()
    {
        if (firstNutTutoDone)
        return;

        if (CM.getCollectedNuts() > 0)
        {
            SpawnWindow(firstNutTutoWindow);
            firstNutTutoDone = true;
        }
    }

    private void SpawnWindow(UITutorialStopWindow iWindow)
    {
        if (!!tutoWindow_Inst)
        Destroy(tutoWindow_Inst.gameObject);
        tutoWindow_Inst = Instantiate(iWindow);
    }
}
}
