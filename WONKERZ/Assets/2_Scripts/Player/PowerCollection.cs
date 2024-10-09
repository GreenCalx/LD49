using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Wonkerz;
[CreateAssetMenu(fileName = "PowerCollection", menuName = "ScriptableObjects/PowerCollection", order = 5)]
public class PowerCollection : ScriptableObject
{
    public static readonly int size = 256;
    public PlayerPowerElement[] powers = new PlayerPowerElement[size];

    public PlayerPowerElement GetPower(int id)
    {
        return powers[id];
    }

    public PlayerPowerElement GetPowerFromCollectible(ONLINE_COLLECTIBLES iCollectibleType)
    {
        foreach (PlayerPowerElement el in powers)
        {
            if (el.relatedCollectible==iCollectibleType)
                return el;
        }
        return null;
    }

}
