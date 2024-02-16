using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CosmeticCollection", order = 3)]
public class CosmeticCollection : ScriptableObject
{
    public static readonly int size = 256;
    public CosmeticElement[] skins = new CosmeticElement[size];

    public CosmeticElement GetCosmetic(int id)
    {
        return skins[id];
    }
}


