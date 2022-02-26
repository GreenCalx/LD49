using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrickChainCondition
{
    public string trickChain1, trickChain2;

    public TrickChainCondition( string iT1Name, string iT2Name)
    {
        trickChain1 = iT1Name;
        trickChain2 = iT2Name;
    }

    public bool checkChain( Trick iT1, Trick iT2)
    {
        if ( (iT1.name == trickChain1) && (iT2.name == trickChain2) )
        { return true; }
        else if ( (iT2.name == trickChain1) && (iT1.name == trickChain2) )
        { return true; }
        return false;
    }
}
