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

    private CollectiblesManager CM;

    // Start is called before the first frame update
    void Start()
    {
        CM = Access.CollectiblesManager();
    }

    // Update is called once per frame
    void Update()
    {
        pollFirstNut();
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
