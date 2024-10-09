using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Wonkerz;
using Schnibble;
/**
*   Cosmetic element
*/
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PlayerPowerElement", order = 5)]
public class PlayerPowerElement : ScriptableObject
{
    public new string name = "";
    public ONLINE_COLLECTIBLES relatedCollectible;
    public Sprite powerImage;
    public GameObject prefabToAttachOnPlayer;
    public float cooldown;

}
