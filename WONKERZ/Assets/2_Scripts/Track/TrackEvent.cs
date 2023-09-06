using System.Collections.Generic;
using UnityEngine;
using Schnibble;

/**
*  Interface with BountyArray 
*  > To be referenced externally in track and set to solved
* 
*/
public class TrackEvent : MonoBehaviour
{
    public string eventID;
    public bool isSolved;


    void Start()
    {
        Access.TrackManager().susbscribe(this); // for bounty check @BountyArray:194
        isSolved = false;
    }

    public void setSolved()
    {
        isSolved = true;

        BountyArray bountyArray = Access.BountyArray();
        BountyArray.AbstractBounty thisAsBounty;
        if (!bountyArray.findBountyFromName(eventID, out thisAsBounty))
        { 
            this.LogError("TrackEvent::setSolved() -> Could not find bounty");
            return;
        }
        if (bountyArray.getStatus(thisAsBounty.x, thisAsBounty.y)==BountyArray.EItemState.UNLOCKED)
        {
            this.Log("TrackEvent::setSolved() -> Bounty already unlocked");
            return;
        }

        bountyArray.updateArray();
        // display bounty UI
        Access.UIBountyUnlocked().display(thisAsBounty);
    }
}