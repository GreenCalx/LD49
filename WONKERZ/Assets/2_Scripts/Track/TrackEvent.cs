using System.Collections.Generic;
using UnityEngine;
using Schnibble;

/**
*  Interface with BountyArray 
*  > To be referenced externally in track and set to solved
* 
*   It can be local to the track, and mark a bounty realization if a UniqueEvent is attached
*/
public class TrackEvent : MonoBehaviour
{
    public string eventID;

    public UniqueEvents.UEVENTS uniqueEventOnSolving  = UniqueEvents.UEVENTS.NONE;
    public bool isSolved;


    void Start()
    {
        Access.TrackManager().susbscribe(this); // for bounty check @BountyArray:194
        isSolved = false;
    }

    public void setSolved()
    {
        if (isSolved)
            return;

        isSolved = true;

        // Notify Progress
        Access.GameProgressSaveManager().notifyUniqueEventDone(uniqueEventOnSolving);

        // Notify BountyMatrix
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