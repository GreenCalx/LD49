using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum CosmeticType 
{
    PAINT = 0,
    MODEL = 1,
    DECAL = 2
}
/**
*   Cosmetic element
*/
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CosmeticElement", order = 2)]
public class CosmeticElement : ScriptableObject
{
    public string name = "";
    public CosmeticType cosmeticType;

    [Header("# Material skins")]
    public Material material;

    [Header("# 3D Model skins")]
    public COLORIZABLE_CAR_PARTS carPart;
    public Mesh mesh;

    [Header("# Jump decal")]
    public GameObject decal;

}