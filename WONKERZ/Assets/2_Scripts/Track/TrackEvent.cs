using UnityEngine;
using Schnibble;

public class TrackEvent : MonoBehaviour
{
    public string eventID;
    public bool isSolved;

    void Start()
    {
        isSolved = false;
        Access.TrackManager().susbscribe(this);
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