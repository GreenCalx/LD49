using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "CosmeticCollection", menuName = "ScriptableObjects/CosmeticCollection", order = 3)]
public class CosmeticCollection : ScriptableObject
{
    public static readonly int size = 256;
    public CosmeticElement[] skins = new CosmeticElement[size];

    public CosmeticElement GetCosmetic(int id)
    {
        return skins[id];
    }

    public CosmeticElement GetCosmeticFromPart(COLORIZABLE_CAR_PARTS iPart)
    {
        foreach(CosmeticElement ce in skins)
        {
            if (ce.carPart == iPart)
            {
                return ce;
            }
        }
        return null;
    }

}


