using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Not MVC cause as this is only use for the intro scene, integrating 
public class IntroTutorialTriggers : MonoBehaviour
{
    public bool firstNutTutoDone = false;
    public UITutorialStopWindow firstNutTutoWindow;

    public bool weightTutoDone = false;
    public UITutorialStopWindow weightTutoWindow;

    public bool getUpTutoDone = false;
    public UITutorialStopWindow getUpTutoWindow;
    public float timeThrStuckDetection = 1f;

    private CollectiblesManager CM;
    private PlayerController PC;

    private float elapsedTimeStuckDetection = 0f;

    // Start is called before the first frame update
    void Start()
    {
        CM = Access.CollectiblesManager();
        PC = Access.Player();

        elapsedTimeStuckDetection = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        pollFirstNut();
        pollPlayerStuck();
    }

    public void pollPlayerStuck()
    {
        if (getUpTutoDone)
            return;
        
        bool playerIsActive = PC.IsInMenu();
        bool playerIsGrounded = PC.TouchGroundAll();
        bool playerHasLowSpeed = PC.car.GetCurrentSpeed() < 5f;

        if (!playerIsActive && !playerIsGrounded && playerHasLowSpeed)
        {
            elapsedTimeStuckDetection += Time.deltaTime;
            if (elapsedTimeStuckDetection >= timeThrStuckDetection)
            {
                Instantiate(getUpTutoWindow);
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
            Instantiate(firstNutTutoWindow);
            firstNutTutoDone = true;
        }
    }
}
