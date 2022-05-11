using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Trick nature alias
using TN = Trick.TRICK_NATURE;

public static class TrickDictionary
{
    public static readonly List<Trick> tricks = 
    
    new List<Trick>
    {

        // NEUTRAL (fillers)
        new Trick( TN.NEUTRAL, "AIR"    , 5     , new TrickCondition(0,0,0)),

        // BASIC FLAT
        new Trick( TN.FLAT, "FRONT WHEELIE",  10, new TrickCondition(new bool[4]{ false, false, true, true})),
        new Trick( TN.FLAT, "BACK WHEELIE",   10,  new TrickCondition(new bool[4]{ true, true, false, false})),
        new Trick( TN.FLAT, "LEFT WHEELIE",   15, new TrickCondition(new bool[4]{ true, false, true, false})),
        new Trick( TN.FLAT, "RIGHT WHEELIE",  15, new TrickCondition(new bool[4]{ false, true, false, true})),

        // BASIC AIRS
        new Trick( TN.BASIC, "1/2 FRONT FLIP"   , 125   , new TrickCondition( -180, 0, 0 ) ),
        new Trick( TN.BASIC, "1/2 BACK FLIP"    , 125   , new TrickCondition( 180, 0, 0 ) ),
        new Trick( TN.BASIC, "1/2 FRONT BARREL" , 75    , new TrickCondition( 0, 0, -180)),
        new Trick( TN.BASIC, "1/2 BACK BARREL"  , 75    , new TrickCondition( 0, 0, 180)),
        new Trick( TN.BASIC, "180 FRONT"        , 40    , new TrickCondition( 0, 180, 0 ) ),
        new Trick( TN.BASIC, "180 BACK"         , 40    , new TrickCondition( 0, -180, 0 ) ),

        // CHAINED
        new Trick( "FRONT FLIP"   , 125   , new TrickChainCondition( "1/2 FRONT FLIP", "1/2 BACK FLIP") ),
        new Trick( "BACK FLIP"    , 125   , new TrickChainCondition( "1/2 BACK FLIP", "1/2 FRONT FLIP" ) ),
        new Trick( "FRONT BARREL" , 75    , new TrickChainCondition( "1/2 FRONT BARREL", "1/2 FRONT BARREL")),
        new Trick( "BACK BARREL"  , 75    , new TrickChainCondition( "1/2 BACK BARREL", "1/2 BACK BARREL")),
        new Trick( "360 FRONT"    , 40    , new TrickChainCondition( "180 FRONT", "180 FRONT" ) ),
        new Trick( "360 BACK"     , 40    , new TrickChainCondition( "180 BACK", "180 BACK" ) ),
        
        new Trick( "540 BACK"     , 90    , new TrickChainCondition( "360 BACK", "180 BACK" ) ),
        new Trick( "540 FRONT"     , 90    , new TrickChainCondition( "360 FRONT", "180 FRONT" ) ),
        
        new Trick( "720 BACK"            , 185    , new TrickChainCondition( "540 BACK", "180 BACK" ) ),
        new Trick( "720 FRONT"           , 185    , new TrickChainCondition( "540 FRONT", "180 FRONT" ) ),
        new Trick( "DOUBLE FRONT BARREL" , 200    , new TrickChainCondition( "FRONT BARREL", "FRONT BARREL")),
        new Trick( "DOUBLE BACK BARREL"  , 200    , new TrickChainCondition( "BACK BARREL", "BACK BARREL")),
        new Trick( "DOUBLE FRONT FLIP"   , 300   , new TrickChainCondition( "FRONT FLIP", "FRONT FLIP") ),
        new Trick( "DOUBLE BACK FLIP"    , 300   , new TrickChainCondition( "BACK FLIP", "BACK FLIP" ) ),
   
        // IGNORE
        new Trick( TN.IGNORE, "ONE WHEEL",   10,  new TrickCondition(new bool[4]{ true, false, false, false})),
        new Trick( TN.IGNORE, "ONE WHEEL",  10, new TrickCondition(new bool[4]{ false, true, false, false})),
        new Trick( TN.IGNORE, "ONE WHEEL",   10, new TrickCondition(new bool[4]{ false, false, true, false})),
        new Trick( TN.IGNORE, "ONE WHEEL",  10, new TrickCondition(new bool[4]{ false, false, false, true}))
    };

    public static readonly Dictionary<TN,int> indexes = initIndexes();

    private static Dictionary<TN,int> initIndexes()
    {
        TN last_nature = TN.UNDEFINED;
        Dictionary<TN,int> retdic = new Dictionary<TN, int>();
        foreach( Trick t in tricks )
        {
            if (t.nature != last_nature)
            {
                last_nature = t.nature;
                retdic.Add( t.nature, tricks.IndexOf(t));
            }
        }

        return retdic;
    }

    public static Trick checkTricksIndexed( TrickTracker iTT, TN iTN)
    {
        int start = 0;

        if ( !indexes.TryGetValue( iTN, out start) )
        {
            Debug.LogWarning("checkTricksIndexed : Input Trick Nature is undefined in indexes.");
            return checkTricks(iTT);
        }

        for( int i = start ; i < tricks.Count ; i++ )
        {
            Trick t = tricks[i];
            if ( t.nature != iTN )
                break;
            if (t.check(iTT))
                return t;
        }
        return null;  
    }

    public static Trick checkChainedTricks(Trick iT1, Trick iT2)
    {
        int start = 0;

        if ( !indexes.TryGetValue( TN.CHAINED, out start) )
        {
            Debug.LogError("checkChainedTricks : Input Trick Nature is undefined in indexes.");
        }

        for( int i = start ; i < tricks.Count ; i++ )
        {
            Trick t = tricks[i];
            if (t.chainCondition!=null)
            {
                if (t.chainCondition.checkChain( iT1, iT2))
                { return t; }
            }
        }
        return null;  
    }

    public static Trick checkTricks( TrickTracker iTT )
    {
        foreach( Trick t in tricks )
        {
            if (t.check(iTT))
                return t;
        }
        return null;
    }

}
