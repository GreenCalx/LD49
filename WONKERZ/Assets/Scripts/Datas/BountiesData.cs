
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class BountiesData
{
    public static BountyArray.AbstractBounty getBountyAt( int iX, int iY)
    {
        // TODO : Binary search
        /*
        int rowLen = BountyArray.N_BOUNTY;

        int start = 0;
        int end = datas.Count - 1;
        while ( start <= end )
        {
            int mid = start + (end-start)/2;
            if (datas[mid].x > iX)
            { end = mid; }
            else if ( datas[mid].x < iX)
            { start = mid; }
            else { // X is gut

            }
        }
        */
        foreach( BountyArray.AbstractBounty d in datas)
        {
            if ((d.x == iX)&&(d.y == iY))
                return d;
        }
        return null;
    }

    // !! Needs to be sorted by X,Y
    public static readonly List<BountyArray.AbstractBounty> datas = new List<BountyArray.AbstractBounty>
    {
        new BountyArray.TrackScoreBounty( 0, 0, "grottotime1", "Beat Grotto in Easy under 1min" , new CosmeticElement("cc_yellow", COLORIZABLE_CAR_PARTS.MAIN, "CarColorYellow", ""), new BountyArray.TrackScoreConstraint(Constants.SN_GROTTO_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 0, 1, "grottotime2", "Beat Grotto in Medium under 1min", new CosmeticElement("cc_pink", COLORIZABLE_CAR_PARTS.MAIN, "CarColorPink", ""), new BountyArray.TrackScoreConstraint(Constants.SN_GROTTO_TRACK, DIFFICULTIES.MEDIUM, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 0, 2, "grottotime3", "Beat Grotto in Hard under 1min",new CosmeticElement("cc_black", COLORIZABLE_CAR_PARTS.MAIN, "CarColorBlack", ""), new BountyArray.TrackScoreConstraint(Constants.SN_GROTTO_TRACK, DIFFICULTIES.HARD, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 0, 3, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 0, 4, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 0, 5, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 0, 6, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 0, 7, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 0, 8, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 0, 9, "grottotimetesto", "test color wheel", new CosmeticElement("wheel_metal", COLORIZABLE_CAR_PARTS.WHEELS, "WheelColorMetal", ""), new BountyArray.TrackScoreConstraint(Constants.SN_GROTTO_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(3,0) ) ),
        
        new BountyArray.TrackScoreBounty( 1, 0, "grottotime4", "Beat Grotto in Ironman under 1min", new CosmeticElement("cc_gold", COLORIZABLE_CAR_PARTS.MAIN, "CarColorGold", ""), new BountyArray.TrackScoreConstraint(Constants.SN_GROTTO_TRACK, DIFFICULTIES.IRONMAN, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 1, 1, "grottotimetesto2", "test skin for car", new CosmeticElement("carskin1", COLORIZABLE_CAR_PARTS.MAIN, "", "Skin1"), new BountyArray.TrackScoreConstraint(Constants.SN_GROTTO_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(5,0) ) ),
        new BountyArray.TrackScoreBounty( 1, 2, "grottotimetesto3", "test skin for wheels", new CosmeticElement("wheelskin1", COLORIZABLE_CAR_PARTS.WHEELS, "", "Skin1"), new BountyArray.TrackScoreConstraint(Constants.SN_GROTTO_TRACK, DIFFICULTIES.MEDIUM, new BountyArray.TimeConstraint(5,0) ) ),
        new BountyArray.TrackScoreBounty( 1, 3, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 1, 4, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 1, 5, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 1, 6, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 1, 7, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 1, 8, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 1, 9, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        
        new BountyArray.TrackScoreBounty( 2, 0, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 2, 1, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 2, 2, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 2, 3, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 2, 4, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 2, 5, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 2, 6, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 2, 7, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 2, 8, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 2, 9, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        
        new BountyArray.TrackScoreBounty( 3, 0, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 3, 1, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 3, 2, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 3, 3, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 3, 4, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 3, 5, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 3, 6, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 3, 7, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 3, 8, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 3, 9, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        
        new BountyArray.TrackScoreBounty( 4, 0, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 4, 1, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 4, 2, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 4, 3, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 4, 4, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 4, 5, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 4, 6, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 4, 7, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 4, 8, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 4, 9, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        
        new BountyArray.TrackScoreBounty( 5, 0, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 5, 1, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 5, 2, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 5, 3, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 5, 4, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 5, 5, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 5, 6, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 5, 7, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 5, 8, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 5, 9, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        
        new BountyArray.TrackScoreBounty( 6, 0, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 6, 1, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 6, 2, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 6, 3, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 6, 4, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 6, 5, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 6, 6, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 6, 7, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 6, 8, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 6, 9, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        
        new BountyArray.TrackScoreBounty( 7, 0, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 7, 1, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 7, 2, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 7, 3, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 7, 4, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 7, 5, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 7, 6, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 7, 7, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 7, 8, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 7, 9, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        
        new BountyArray.TrackScoreBounty( 8, 0, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 8, 1, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 8, 2, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 8, 3, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 8, 4, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 8, 5, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 8, 6, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 8, 7, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 8, 8, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 8, 9, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        
        new BountyArray.TrackScoreBounty( 9, 0, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 9, 1, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 9, 2, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 9, 3, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 9, 4, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 9, 5, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 9, 6, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 9, 7, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 9, 8, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) ),
        new BountyArray.TrackScoreBounty( 9, 9, "default", "default", new CosmeticElement(), new BountyArray.TrackScoreConstraint(Constants.SN_JUNKYARD_TRACK, DIFFICULTIES.EASY, new BountyArray.TimeConstraint(1,0) ) )
    };


}