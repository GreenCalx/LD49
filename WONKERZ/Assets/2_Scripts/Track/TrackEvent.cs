using System.Collections.Generic;
using System;
using UnityEngine;
using Schnibble;

namespace Wonkerz {
    /**
     *  Interface with BountyArray
     *  > To be referenced externally in track and set to solved
     *
     *   It can be local to the track, and mark a bounty realization if a UniqueEvent is attached
     */
    public class TrackEvent : MonoBehaviour
    {
        //public string eventID;

        public UniqueEvents.UEVENTS uniqueEventOnSolving  = UniqueEvents.UEVENTS.NONE;
        public bool isSolved;


        void Start()
        {
            Access.managers.trackMgr.susbscribe(this); // for bounty check @BountyArray:194
            isSolved = false;
        }

        public void setSolved()
        {
            if (isSolved)
            return;

            isSolved = true;

            // Notify Progress
            Access.managers.gameProgressSaveMgr.notifyUniqueEventDone(uniqueEventOnSolving);

            // Notify BountyMatrix
            BountyArray bountyArray = Access.managers.bountyArray;

            BountyArray.AbstractBounty thisAsBounty;
            string bountyName = UniqueEvents.GetEventName(uniqueEventOnSolving);
            if (!bountyArray.findBountyFromUEvent(bountyName, out thisAsBounty))
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
    }}
