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

    void Update()
    {

    }

    void OnTriggerEnter(Collider iCollider)
    {
        if (triggered)
            return;

        if (Utils.colliderIsPlayer(iCollider))
        {
            tutorialBalloon.updateDisplay(elements);
            triggered = true;
        }
    }
}
