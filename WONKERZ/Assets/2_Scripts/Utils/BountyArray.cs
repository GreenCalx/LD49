using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using Schnibble;
using Schnibble.UI;


[System.Serializable]
public class BountyUnlockMatrixData : EntityData
{
    public SerializableBountyUnlockMatrix loadedBountyUnlockMatrix;

    public override void OnLoad(GameObject gameObject)
    {
        BountyArray ba = Access.BountyArray();
        if (!!ba)
        {
            // 
            //ba.unlockMatrix.xyz       = loadedBountyUnlockMatrix.xyz;
            ba.bountyMatrix.bountiesUnlockStatus = loadedBountyUnlockMatrix.bountiesUnlockStatus;

            ba.bountyMatrix.bounty_unlock_matrix_data = this;
        }
    }
}

[System.Serializable]
public class SerializableBountyUnlockMatrix
{
    public BountyArray.EItemState[,] bountiesUnlockStatus = new BountyArray.EItemState[BountyArray.N_BOUNTY, BountyArray.N_BOUNTY];

    public BountyUnlockMatrix buildBountyUnlockMatrix()
    {
        BountyUnlockMatrix retval = new BountyUnlockMatrix();
        
        retval.bountiesUnlockStatus = bountiesUnlockStatus;

        return retval;
    }

    public BountyUnlockMatrix BountyUnlockMatrix
    {
        get 
        { 
            return buildBountyUnlockMatrix(); 
        }
        set
        { 
            bountiesUnlockStatus = value.bountiesUnlockStatus;
        }
    }
    public static implicit operator BountyUnlockMatrix(SerializableBountyUnlockMatrix inst)
    {
        return inst.BountyUnlockMatrix;
    }
    public static implicit operator SerializableBountyUnlockMatrix(BountyUnlockMatrix iTS)
    {
        return new SerializableBountyUnlockMatrix { BountyUnlockMatrix = iTS };
    }
}

///////
[Serializable]
public class BountyUnlockMatrix : ISaveLoad
{
    public BountyArray.EItemState[,] bountiesUnlockStatus = new BountyArray.EItemState[BountyArray.N_BOUNTY, BountyArray.N_BOUNTY];

    // save
    public BountyUnlockMatrixData bounty_unlock_matrix_data;

    object ISaveLoad.GetData()
    {
        if (bounty_unlock_matrix_data==null)
            bounty_unlock_matrix_data = new BountyUnlockMatrixData();

        bounty_unlock_matrix_data.loadedBountyUnlockMatrix = this;

        return bounty_unlock_matrix_data;
    }
}

///
public class BountyArray : MonoBehaviour
{
    [Header("UI")]
    public GameObject UICheckListItem;

    public static readonly int N_BOUNTY = 10;
    public enum EItemState { LOCKED=0, VISIBLE=1, UNLOCKED=2 };

    public BountyUnlockMatrix bountyMatrix;

    public AbstractBounty[,] bounties = new AbstractBounty[N_BOUNTY,N_BOUNTY];

    ///
    [Serializable]
    public abstract class AbstractBounty
    {
        public string name; //  key to retrieve bounty in the garage and such
        public int x, y;
        public int[] cosmeticBountyIDs;
        public string hint;

        public AbstractBounty(int iX, int iY, string iName, string iHint , int[] iCosmeticBountyIDs)
        {
            name    = iName;
            x       = iX;
            y       = iY;
            cosmeticBountyIDs  = iCosmeticBountyIDs;
            hint    = iHint;
        }

        virtual public bool check() { return false; }

        public List<CosmeticElement> GetAttachedCosmetics()
        {
            return Access.PlayerCosmeticsManager()?.getCosmeticsFromIDs(cosmeticBountyIDs);
        }

        public string GetRewardsAsText()
        {
            string reward_txt = "";
            foreach ( CosmeticElement ce in GetAttachedCosmetics())
            { reward_txt += ce.name; reward_txt += '\n'; }
            return reward_txt;
        }
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
    public struct EventTriggerConstraint
    {
        public string track_name;
        public UniqueEvents.UEVENTS eventID;
        public EventTriggerConstraint(string iTName, UniqueEvents.UEVENTS iEventID)
        {
            eventID     = iEventID;
            track_name  = iTName;
        }
    }

    ///
    [Serializable]
    public class TrackScoreBounty : AbstractBounty
    {
        public TrackScoreConstraint tsc;

        public TrackScoreBounty(int iX, int iY, string iName, string iHint , int[] iBountyCosmeticID, TrackScoreConstraint iTSC) : base(iX, iY, iName, iHint, iBountyCosmeticID)
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

    [Serializable]
    public class TrackEventBounty : AbstractBounty
    {
        public EventTriggerConstraint etc;
        public TrackEventBounty(int iX, int iY, string iName, string iHint , int[] iBountyCosmeticID, EventTriggerConstraint iETC) : base(iX, iY, iName, iHint, iBountyCosmeticID)
        {
            etc = iETC;
        }
        override public bool check()
        {
            // check etc
            // TrackManager TM = Access.TrackManager();
            // if (TM.launchedTrackName != etc.track_name)
            //     return false;

            // foreach( TrackEvent e in TM.track_events )
            // {
            //     if (e.eventID == etc.eventID)
            //     {
            //         return e.isSolved;
            //     }
            // }
            return Access.GameProgressSaveManager().IsUniqueEventDone(etc.eventID);
            //return false;
        }
    }

    void Start()
    {
        loadBounties();
        checkBounties();
        updateArray(); // collectibles manager might not be inited

    }

    public EItemState getStatus( int iX, int iY) { return bountyMatrix.bountiesUnlockStatus[iX,iY]; }

    private void loadBounties()
    {
        for(int i = 0; i<N_BOUNTY; i++)
        {
            for (int j=0; j<N_BOUNTY; j++)
            {
                bounties[i,j] = BountiesData.getBountyAt(i, j);
            }
        }
        loadBountyMatrix();
    }

    private void checkBounties()
    {
        // useless now?
    }

    // TODO : O(2N²) iz bad
    public void updateArray()
    {
        PlayerCosmeticsManager pcm = Access.PlayerCosmeticsManager();
        if (pcm==null)
        { UnityEngine.Debug.LogError("BountyArray::updateArray:: no Player cosmetics manager"); return;}

        // Get unlocks
        for(int i = 0; i<N_BOUNTY; i++)
        {
            for (int j=0; j<N_BOUNTY; j++)
            {                
                if (bountyMatrix.bountiesUnlockStatus[i,j]==EItemState.UNLOCKED)
                {
                    // Already unlocked, nothing to do
                    pcm.addCosmetic(bounties[i,j].cosmeticBountyIDs);
                    continue;
                }

                if (bounties[i,j].check())
                {   // new unlock !
                    bountyMatrix.bountiesUnlockStatus[i,j] = EItemState.UNLOCKED;
                    pcm.addCosmetic(bounties[i,j].cosmeticBountyIDs);

                    saveBountyMatrix();
                    continue; 
                }
            }
        }

        // Make unlocked neighbors visible
        for(int i = 0; i<N_BOUNTY; i++)
        {
            for (int j=0; j<N_BOUNTY; j++)
            {
                if (bountyMatrix.bountiesUnlockStatus[i,j]==EItemState.UNLOCKED)
                {
                    // UP
                    if ( (i>0) && bountyMatrix.bountiesUnlockStatus[i-1,j]!=EItemState.UNLOCKED )
                    { bountyMatrix.bountiesUnlockStatus[i-1,j] = EItemState.VISIBLE; }
                    // LEFT
                    if ( (j>0) && bountyMatrix.bountiesUnlockStatus[i,j-1]!=EItemState.UNLOCKED )
                    { bountyMatrix.bountiesUnlockStatus[i,j-1] = EItemState.VISIBLE; }
                    // DOWN
                    if ( (i<N_BOUNTY-1) && bountyMatrix.bountiesUnlockStatus[i+1,j]!=EItemState.UNLOCKED )
                    { bountyMatrix.bountiesUnlockStatus[i+1,j] = EItemState.VISIBLE; }
                    // RIGHT
                    if ( (j<N_BOUNTY-1) && bountyMatrix.bountiesUnlockStatus[i,j+1]!=EItemState.UNLOCKED )
                    { bountyMatrix.bountiesUnlockStatus[i,j+1] = EItemState.VISIBLE; }
                }
            }
        }
    }

    public bool checkItem() // ??
    {
        return false;
    }

    public void initUI(UIPanelTabbed parentUI, TextMeshProUGUI iToolTip_desc, TextMeshProUGUI iToolTip_name, TextMeshProUGUI iToolTip_reward)
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

                uictt.x = i;
                uictt.y = j;

                uictt.bountyArray = this;

                // Color
                uictt.updateColor();
                uictt.tooltip_bountyDesc    = iToolTip_desc;
                uictt.tooltip_bountyName    = iToolTip_name;
                uictt.tooltip_bountyReward  = iToolTip_reward;

                parentUI.tabs.Add(uictt);
            }
        }
        parentUI.SelectTab(0);
    }

    public bool getBountyAt(int x, int y, out AbstractBounty oBounty)
    {
        oBounty = null;

        if (bountyMatrix.bountiesUnlockStatus[x,y] != EItemState.LOCKED)
        { oBounty = bounties[x,y]; }

        return (oBounty != null);
    }

    public bool findBountyFromName( string iName, out AbstractBounty oBounty )
    {
        oBounty = null;
        for(int i = 0; i<N_BOUNTY; i++)
        {
            for (int j=0; j<N_BOUNTY; j++)
            {   
                AbstractBounty bounty = bounties[i,j];
                if (bounty.name == iName)
                {
                    oBounty = bounty;
                    break;
                }
            }
        }
        return (oBounty != null);
    }

    public bool findBountyFromUEvent(string iUEventName, out AbstractBounty oBounty )
    {
        oBounty = null;
        for(int i = 0; i<N_BOUNTY; i++)
        {
            for (int j=0; j<N_BOUNTY; j++)
            {   
                try {
                    TrackEventBounty bounty =(TrackEventBounty)bounties[i,j];
                    if (bounty!=null)
                    {
                        EventTriggerConstraint bnty_tsc = bounty.etc;
                        string event_name = UniqueEvents.GetEventName(bnty_tsc.eventID);
                        if (event_name == iUEventName)
                        {
                            oBounty = bounty;
                            break;
                        }
                    }
                } catch (InvalidCastException ice) { continue; }

            }
        }
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

    public void saveBountyMatrix()
    {
        SaveAndLoad.datas.Add(bountyMatrix);
        SaveAndLoad.save(Constants.FD_BOUNTYMATRIX);
    }

    public void loadBountyMatrix()
    {
        SaveAndLoad.loadBountyMatrix(Constants.FD_BOUNTYMATRIX, this);
    }

}
