using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;

public class TutorialBalloonTrigger : MonoBehaviour
{
    [Header("MAND")]
    public TutorialBalloon tutorialBalloon;
    public List<UIGenerativeTextBox.UIGTBElement> elements;
    private bool triggered = false;
    public bool disableBalloon = false;
    public bool triggerOnce = false;

    void Update()
    {

    }

    void OnTriggerEnter(Collider iCollider)
    {
        if (triggerOnce)
        {
            if (triggered)
                return;
        }

        if (disableBalloon)
        {
            tutorialBalloon.enable_move = false;
            tutorialBalloon.disable_balloon_follow = true;
        }

        if (Utils.colliderIsPlayer(iCollider))
        {
            tutorialBalloon.updateDisplay(elements);
            triggered = true;
        }
    }
}
