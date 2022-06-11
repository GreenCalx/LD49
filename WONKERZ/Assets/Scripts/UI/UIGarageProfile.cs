using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SerializableColor
{
    public float[] colorStore = new float[4]{1F,1F,1F,1F};
    public Color Color {
        get{ return new Color( colorStore[0], colorStore[1], colorStore[2], colorStore[3] );}
        set{ colorStore = new float[4]{ value.r, value.g, value.b, value.a  };}
    }
    public static implicit operator Color( SerializableColor inst )
    {
        return inst.Color;
    }
    public static implicit operator SerializableColor( Color iColor)
    {
        return new SerializableColor{ Color = iColor };
    }
}


[System.Serializable]
public class ProfileData : EntityData
{
    //public AnimationCurve ac_torque;
    //public AnimationCurve ac_weight;
    public SerializableColor color;
    public override void OnLoad(GameObject gameObject)
    {
        UIGarageProfile uigp = gameObject.GetComponent<UIGarageProfile>();
        if (!!uigp)
        {
            //uigp.TORQUE_CURVE   = ac_torque;
            //uigp.WEIGHT_CURVE   = ac_weight;
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
        //profileData.ac_torque = TORQUE_CURVE;
        //profileData.ac_weight = WEIGHT_CURVE;
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