using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class ProfileData : EntityData
{
    public AnimationCurve ac_torque;
    public AnimationCurve ac_weight;
    public Color          color;
    public override void OnLoad(GameObject gameObject)
    {
        UIGarageProfile uigp = gameObject.GetComponent<UIGarageProfile>();
        if (!!uigp)
        {
            uigp.TORQUE_CURVE   = ac_torque;
            uigp.WEIGHT_CURVE   = ac_weight;
            uigp.color          = color;

            uigp.profileData = this;

        } else {
            Debug.LogError("Failed to retrieve UIGarageProfile OnLoad()");
        }
    }
}

public class UIGarageProfile : MonoBehaviour, ISaveLoad
{
    [Header("MANDATORY")]
    public string fileName;

    [Header("datas")]
    // CAR STATS
    [HideInInspector]
    public AnimationCurve TORQUE_CURVE;
    [HideInInspector]
    public AnimationCurve WEIGHT_CURVE;

    // Cosmetics
    [HideInInspector]
    public Color color;

    // Serizable datas
    public ProfileData profileData;

    UIGarageProfile()
    {

    }

    object ISaveLoad.GetData()
    {
        profileData.ac_torque = TORQUE_CURVE;
        profileData.ac_weight = WEIGHT_CURVE;
        profileData.color     = color;

        return profileData;
    }

    void Start()
    {
        SaveAndLoad.datas.Add(this);
    }
    void OnDestroy()
    {
        SaveAndLoad.datas.Remove(this);
    }
}