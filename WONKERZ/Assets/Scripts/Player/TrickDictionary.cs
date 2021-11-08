using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TrickDictionary
{
    public static readonly List<Trick> tricks = 
    
    new List<Trick>
    {
        new Trick("AIR", 5, new TrickCondition(0,0,0)) ,

        new Trick("FRONT WHEELIE",  10, new TrickCondition(new bool[4]{ false, false, true, true})),
        new Trick("BACK WHEELIE",   10,  new TrickCondition(new bool[4]{ true, true, false, false})),
        new Trick("LEFT WHEELIE",   15, new TrickCondition(new bool[4]{ true, false, true, false})),
        new Trick("RIGHT WHEELIE",  15, new TrickCondition(new bool[4]{ false, true, false, true}))

    };

    public static Trick checkTricks( TrickTracker iTT )
    {
        foreach( Trick t in tricks )
        {
            if (t.condition.check(iTT))
                return t;
        }
        return null;
    }

}
