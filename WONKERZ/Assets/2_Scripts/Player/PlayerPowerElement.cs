using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Wonkerz;
using Schnibble;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PlayerPowerElement", order = 5)]
public class PlayerPowerElement : ScriptableObject
{
    [Header("Generics")]
    public new string name = "";
    public ONLINE_COLLECTIBLES relatedCollectible;
    public Sprite powerImage;
    public GameObject prefabToAttachOnPlayer;
    [Tooltip("Cooldown between power activations in seconds.")]
    public float cooldown;
    [Tooltip("Execution time of the power in seconds. Cooldown starts when recovery is over.")]
    public float recovery;
    [Tooltip("Direct damage dealt by weapon or its effect/projectile")]
    public int baseDamage;
    [Header("Specialization")]
    [SerializeReference, SubclassPicker]
    public ICarPower carPower;

}
