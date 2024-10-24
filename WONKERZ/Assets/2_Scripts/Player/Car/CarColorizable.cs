using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum COLORIZABLE_CAR_PARTS {
    MAIN=0 ,
    LEFT_DOOR=1,
    RIGHT_DOOR=2,
    FRONT_BUMP=3,
    BACK_BUMP=4,
    HOOD=5,
    WHEELS=6,
    SPRINGS=7,
    LAMPS=8,
    FRONT_PIPES=9,
    BACK_PIPES=10,
    WINDSHIELD=11,
    WEIGHT=12,
    JUMP_DECAL=13,
    ANY=14
}

public class CarColorizable : MonoBehaviour
{
    public COLORIZABLE_CAR_PARTS part;

    public int materialSkinID;
    public int partSkinID;
    // public string partSkinName;
    // public string materialName;
}
