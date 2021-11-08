using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trick
{
    public string name;
    public int value;
    public TrickCondition condition;

    public Trick( string iName, int iValue, TrickCondition iCond)
    {
        name = iName;
        value = iValue;
        condition = iCond;
    }
}
