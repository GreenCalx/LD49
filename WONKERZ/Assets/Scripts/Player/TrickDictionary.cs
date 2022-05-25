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
        new Trick( TN.FLAT, "FS WHEELIE",  10, new TrickCondition(new bool[4]{ false, false, true, true})),
        new Trick( TN.FLAT, "BS WHEELIE",   10,  new TrickCondition(new bool[4]{ true, true, false, false})),
        new Trick( TN.FLAT, "LEFT WHEELIE",   15, new TrickCondition(new bool[4]{ true, false, true, false})),
        new Trick( TN.FLAT, "RIGHT WHEELIE",  15, new TrickCondition(new bool[4]{ false, true, false, true})),

        // BASIC AIRS
        new Trick( TN.BASIC, "HALF FS FLIP"   , 125   , new TrickCondition( 90, 0, 0 ) ),
        new Trick( TN.BASIC, "HALF BS FLIP"    , 125   , new TrickCondition( -90, 0, 0 ) ),
        new Trick( TN.BASIC, "HALF FS BARREL" , 75    , new TrickCondition( 0, 0, -90)),
        new Trick( TN.BASIC, "HALF BS BARREL"  , 75    , new TrickCondition( 0, 0, 90)),
        new Trick( TN.BASIC, "180 FS"        , 40    , new TrickCondition( 0, 90, 0 ) ),
        new Trick( TN.BASIC, "180 BS"         , 40    , new TrickCondition( 0, -90, 0 ) ),

        // CHAINED
            // classics
        new Trick( "FS FLIP"   , 125   , new TrickChainCondition( "HALF FS FLIP", "HALF FS FLIP") ),
        new Trick( "BS FLIP"    , 125   , new TrickChainCondition( "HALF BS FLIP", "HALF BS FLIP" ) ),
        new Trick( "FS BARREL" , 75    , new TrickChainCondition( "HALF FS BARREL", "HALF BS BARREL")),
        new Trick( "BS BARREL"  , 75    , new TrickChainCondition( "HALF BS BARREL", "HALF FS BARREL")),
        new Trick( "360 FS"    , 40    , new TrickChainCondition( "180 FS", "180 FS" ) ),
        new Trick( "360 BS"     , 40    , new TrickChainCondition( "180 BS", "180 BS" ) ),
        
        new Trick( "540 BS"     , 90    , new TrickChainCondition( "360 BS", "180 BS" ) ),
        new Trick( "540 FS"     , 90    , new TrickChainCondition( "360 FS", "180 FS" ) ),
        
        new Trick( "720 BS"            , 185    , new TrickChainCondition( "540 BS", "180 BS" ) ),
        new Trick( "720 FS"           , 185    , new TrickChainCondition( "540 FS", "180 FS" ) ),
        new Trick( "DOUBLE FS BARREL" , 200    , new TrickChainCondition( "FS BARREL", "FS BARREL")),
        new Trick( "DOUBLE BS BARREL"  , 200    , new TrickChainCondition( "BS BARREL", "BS BARREL")),
        new Trick( "DOUBLE FS FLIP"   , 300   , new TrickChainCondition( "FS FLIP", "FS FLIP") ),
        new Trick( "DOUBLE BS FLIP"    , 300   , new TrickChainCondition( "BS FLIP", "BS FLIP" ) ),

            // fancy
        new Trick( "FS TWIST"  , 185   , new TrickChainCondition( "HALF FS FLIP", "180 FS") ),
        new Trick( "SWEEPER"      , 132   , new TrickChainCondition( "180 BS", "180 FS" ) ),
        new Trick( "BS TWIST"   , 185    , new TrickChainCondition( "HALF BS FLIP", "180 BS")),
        new Trick( "FS CORK"  , 122    , new TrickChainCondition( "180 FS", "HALF FS BARREL")),
        new Trick( "BS CORK"    , 122    , new TrickChainCondition( "180 BS", "HALF BS BARREL" ) ),
        new Trick( "FS UNDERFLIP"     , 168    , new TrickChainCondition( "HALF FS BARREL", "HALF FS FLIP" ) ),
        new Trick( "BS UNDERFLIP"     , 168    , new TrickChainCondition( "HALF BS BARREL", "HALF BS FLIP" ) ),
        new Trick( "FS CORKTWIST"     , 321    , new TrickChainCondition( "FS CORK", "FS TWIST" ) ),
        new Trick( "BS CORKTWIST"     , 321    , new TrickChainCondition( "BS CORK", "BS TWIST" ) ),

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
