using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum COLORIZABLE_CAR_PARTS {
    MAIN,
    DOORS,
    BUMPS,
    HOOD,
    WHEELS
}

public class CarColorizable : MonoBehaviour
{
    public COLORIZABLE_CAR_PARTS part;
}
