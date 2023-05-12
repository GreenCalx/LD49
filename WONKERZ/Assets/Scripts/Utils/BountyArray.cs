using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using Schnibble;

public class BountyArray : MonoBehaviour
{
    [Header("UI")]
    public GameObject UICheckListItem;

    public static readonly int N_BOUNTY = 10;
    public enum EItemState { LOCKED=0, VISIBLE=1, UNLOCKED=2 };

    public EItemState[,] bountiesUnlockStatus = new EItemState[N_BOUNTY, N_BOUNTY];

    public AbstractBounty[,] bounties = new AbstractBounty[N_BOUNTY,N_BOUNTY];

    ///
    [Serializable]
    public abstract class AbstractBounty
    {
        public string name; //  key to retrieve bounty in the garage and such
        public int x, y;
        public CosmeticElement cosmeticBounty;
        public string hint;

        public AbstractBounty(int iX, int iY, string iName, string iHint , CosmeticElement iCosmeticBounty)
        {
            name    = iName;
            x       = iX;
            y       = iY;
            cosmeticBounty  = iCosmeticBounty;
            hint    = iHint;
        }

        virtual public bool check() { return false; }
    }

    ///
    [Serializable]
    public struct TimeConstraint
    {
        public int minutes;
        public int seconds;
        public TimeConstraint(int iM,int iS)
        {
            minutes = iM;
            seconds = iS;
        }
    }

    ///
    [Serializable]
    public class TrackScoreConstraint
    {
        public DIFFICULTIES     difficulty;
        public TimeConstraint   time;
        public string track_name;

        public TrackScoreConstraint(string iTName, DIFFICULTIES iDiff, TimeConstraint iTC)
        {
            difficulty = iDiff;
            time = iTC;
            track_name = iTName;
        }
    }

    ///
    [Serializable]
    public class TrackScoreBounty : AbstractBounty
    {
        public TrackScoreConstraint tsc;

        public TrackScoreBounty(int iX, int iY, string iName, string iHint , CosmeticElement iBounty, TrackScoreConstraint iTSC) : base(iX, iY, iName, iHint, iBounty)
        {
            tsc = iTSC;
        }

        override public bool check()
        {
            double best_time = Access.TrackManager().getRacePB(tsc.track_name, tsc.difficulty);
            int min = (int)(best_time / 60);
            int sec = (int)(best_time % 60);
            if (min < tsc.time.minutes)
                return true;
            if (sec < tsc.time.seconds)
                return true;

            return false;
        }
    }

    void Start()
    {
        loadBounties();
        checkBounties();
        updateArray(); // collectibles manager might not be inited

    }

    public EItemState getStatus( int iX, int iY) { return bountiesUnlockStatus[iX,iY]; }

    private void loadBounties()
    {
        for(int i = 0; i<N_BOUNTY; i++)
        {
            for (int j=0; j<N_BOUNTY; j++)
            {
                bounties[j,i] = BountiesData.getBountyAt(i, j);
            }
        }

    }

    private void checkBounties()
    {
        // useless now?
    }

    // TODO : O(2N²) iz bad
    private void updateArray()
    {
        // Get unlocks
        for(int i = 0; i<N_BOUNTY; i++)
        {
            for (int j=0; j<N_BOUNTY; j++)
            {                
                if (bounties[i,j].check())
                { 
                    bountiesUnlockStatus[i,j] = EItemState.UNLOCKED;
                    Access.PlayerCosmeticsManager().addCosmetic(bounties[i,j].cosmeticBounty);
                    continue; 
                }
            }
        }

        // Make unlocked neighbors visible
        for(int i = 0; i<N_BOUNTY; i++)
        {
            for (int j=0; j<N_BOUNTY; j++)
            {
                if (bountiesUnlockStatus[i,j]==EItemState.UNLOCKED)
                {
                    // UP
                    if ( (i>0) && bountiesUnlockStatus[i-1,j]!=EItemState.UNLOCKED )
                    { bountiesUnlockStatus[i-1,j] = EItemState.VISIBLE; }
                    // LEFT
                    if ( (j>0) && bountiesUnlockStatus[i,j-1]!=EItemState.UNLOCKED )
                    { bountiesUnlockStatus[i,j-1] = EItemState.VISIBLE; }
                    // DOWN
                    if ( (i<N_BOUNTY-1) && bountiesUnlockStatus[i+1,j]!=EItemState.UNLOCKED )
                    { bountiesUnlockStatus[i+1,j] = EItemState.VISIBLE; }
                    // RIGHT
                    if ( (j<N_BOUNTY-1) && bountiesUnlockStatus[i,j+1]!=EItemState.UNLOCKED )
                    { bountiesUnlockStatus[i,j+1] = EItemState.VISIBLE; }
                }
            }
        }
    }

    public bool checkItem() // ??
    {
        return false;
    }

    public void initUI(UIPanelTabbed parentUI, TextMeshProUGUI iToolTip)
    {
        updateArray();

        // invoke checkboxes and such
        for(int i = 0; i<N_BOUNTY; i++)
        {
            for (int j=0; j<N_BOUNTY; j++)
            {
                GameObject item = Instantiate(UICheckListItem, parentUI.transform);

                // Add them to parent 'tabs'
                UIChecklistImageTab uictt = item.GetComponent<UIChecklistImageTab>();
                if (uictt==null)
                { this.Log("Image tab not found for bounty matrix."); break; }
                
                uictt.Parent = parentUI;
                uictt.copyColorFromParent = true;
                uictt.copyInputsFromParent = true;
                uictt.init();

                uictt.x = j;
                uictt.y = i;

                uictt.bountyArray = this;

                // Color
                uictt.updateColor();
                uictt.tooltip = iToolTip;

                parentUI.tabs.Add(uictt);
            }
        }
        parentUI.SelectTab(0);
    }

    public bool getBountyAt(int x, int y, out AbstractBounty oBounty)
    {
        oBounty = null;

        if (bountiesUnlockStatus[x,y] != EItemState.LOCKED)
        { oBounty = bounties[x,y]; }

        return (oBounty != null);
    }

    public void hide(UIPanelTabbed parentUI)
    {
        foreach(var o in parentUI.tabs)
        {
            Destroy(o.gameObject);
        }
        parentUI.tabs.Clear();
    }


}