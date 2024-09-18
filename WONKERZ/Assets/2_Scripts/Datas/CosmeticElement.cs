using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum CosmeticType 
{
    PAINT = 0,
    MODEL = 1,
    DECAL = 2,
    RUBBER= 3
}
/**
*   Cosmetic element
*/
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CosmeticElement", order = 2)]
public class CosmeticElement : ScriptableObject
{
    public new string name = "";
    public CosmeticType cosmeticType;

    public bool isDefaultSkin = false;

    [Header("# Material skins")]
    public Material material;

    [Header("# 3D Model skins")]
    public COLORIZABLE_CAR_PARTS carPart;
    public Mesh mesh;

    [Header("# Jump decal")]
    public GameObject decal;

    [Header("Internals")]
    public int skinID = 0;

}
