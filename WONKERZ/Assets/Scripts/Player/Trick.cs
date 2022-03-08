using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trick
{
    public enum TRICK_TYPE   { GROUND=0, AIR=1 };
    public enum TRICK_NATURE {  UNDEFINED=0, 
                                NEUTRAL=1, 
                                FLAT=2, 
                                BASIC =3,
                                CHAINED = 4,
                                IGNORE = 5
                            };
    public string name;
    public int value;
    public TrickCondition condition;
    public TrickChainCondition chainCondition;

    public TRICK_TYPE type;
    public TRICK_NATURE nature;

    public Trick( TRICK_NATURE iNature, string iName, int iValue, TrickCondition iCond)
    {
        name            = iName;
        value           = iValue;
        condition       = iCond;
        chainCondition  = null;
        nature          = iNature;

        deduceType(iCond);
    }

    public Trick( string iName, int iValue, TrickChainCondition iCond, TRICK_NATURE iNature = TRICK_NATURE.CHAINED)
    {
        name        = iName;
        value       = iValue;
        chainCondition   = iCond;
        condition = null;
        nature      = iNature;
    }

    private void deduceType(TrickCondition iTC)
    {
        if ( iTC.wheels != null)
        {
            foreach ( bool wheelOnGround in iTC.wheels )
            {
                if (wheelOnGround)
                {
                    type = TRICK_TYPE.GROUND;
                    return;
                }
            }
        }

        // if not ground, is air.
        type = TRICK_TYPE.AIR;
        return;
    }

    public bool check(TrickTracker iTT)
    {
        if ( type == TRICK_TYPE.GROUND )
        {
            return condition.compare_wheels(iTT);
        } 
        else if ( type == TRICK_TYPE.AIR )
        {
            return condition.compare_air(iTT);
        }

        return false;
    }

    public void chain( Trick iOther )
    {

    }

    public bool isNeutral()
    {
        return nature == TRICK_NATURE.NEUTRAL;
    }
    public bool isBasic()
    {
        return nature == TRICK_NATURE.BASIC;
    }
    public bool isFlat()
    {
        return nature == TRICK_NATURE.FLAT;
    }

    public bool isChained()
    {
        return nature == TRICK_NATURE.CHAINED;
    }
}
