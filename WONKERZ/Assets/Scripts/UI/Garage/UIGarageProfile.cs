using System.Collections.Generic;
using UnityEngine;
using Schnibble;

[System.Serializable]
public class SerializableSkin
{
    public int carPartID; // CarPartToColorize
    public string partSkinName; // id of the skin for a given car part

    public CarColorizable buildSkin()
    {
        CarColorizable retval = new CarColorizable();
        retval.part = (COLORIZABLE_CAR_PARTS) carPartID;
        retval.partSkinName = partSkinName;
        return retval;
    }
    public CarColorizable CarColorizable
    {
        get 
        { 
            return buildSkin();
        } 
        set 
        { 
            carPartID = (int) value.part; 
            partSkinName = value.partSkinName;
        }
    }
    public static implicit operator CarColorizable(SerializableSkin inst)
    {
        return inst.CarColorizable;
    }
    public static implicit operator SerializableSkin(CarColorizable iSkin)
    {
        return new SerializableSkin { CarColorizable = iSkin };
    }
}

[System.Serializable]
public class SerializableColor
{
    public byte[] colorStore = new byte[4] { 0xFF, 0xFF, 0xFF, 0xFF};
    public Color32 Color
    {
        get { return new Color32(colorStore[0], colorStore[1], colorStore[2], colorStore[3]); }
        set { colorStore = new byte[4] { value.r, value.g, value.b, value.a }; }
    }
    public static implicit operator Color32(SerializableColor inst)
    {
        return inst.Color;
    }
    public static implicit operator SerializableColor(Color32 iColor)
    {
        return new SerializableColor { Color = iColor };
    }
}

[System.Serializable]
public class SerializableKeyframe
{
    public float time;
    public float tvalue;
    public float inTangent;
    public float outTangent;
    public float inWeight;
    public float outWeight;

    public Keyframe buildKeyFrame()
    {
        Keyframe retval = new Keyframe(time, tvalue);
        retval.inTangent = inTangent;
        retval.outTangent = outTangent;
        retval.inWeight = inWeight;
        retval.outWeight = outWeight;
        return retval;
    }
    // ! Weighted mode not serialized !
    public Keyframe Keyframe
    {
        get
        {
            return buildKeyFrame();
        }
        set
        {
            time = value.time;
            tvalue = value.value;
            inTangent = value.inTangent;
            outTangent = value.outTangent;
            inWeight = value.inWeight;
            outWeight = value.outWeight;
        }
    }

    public static implicit operator Keyframe(SerializableKeyframe inst)
    {
        return inst.Keyframe;
    }
    public static implicit operator SerializableKeyframe(Keyframe iKey)
    {
        return new SerializableKeyframe { Keyframe = iKey };
    }
}


[System.Serializable]
public class ProfileData : EntityData
{
    public List<SerializableKeyframe> ac_torque;
    public List<SerializableKeyframe> ac_weight;

    //cosmetics
    public SerializableColor color;
    public SerializableColor color_bumps;
    public SerializableColor color_doors;
    public SerializableColor color_hood;
    public SerializableColor color_wheels;

    public SerializableSkin skin_body;
    public SerializableSkin skin_back_bump;
    public SerializableSkin skin_front_bump;
    public SerializableSkin skin_left_door;
    public SerializableSkin skin_right_door;
    public SerializableSkin skin_hood;
    public SerializableSkin skin_wheel;

    public override void OnLoad(GameObject gameObject)
    {
        UIGarageProfile uigp = gameObject.GetComponent<UIGarageProfile>();
        if (!!uigp)
        {
            // TORQUE
            //uigp.TORQUE_CURVE = new List<Keyframe>();
            //foreach (SerializableKeyframe sk in ac_torque)
            //{ uigp.TORQUE_CURVE.Add(sk); }

            // WEIGHT
            // uigp.WEIGHT_CURVE = new List<Keyframe>();
            // foreach (SerializableKeyframe sk in ac_weight)
            // { uigp.WEIGHT_CURVE.Add(sk); }

            // COLOR
            uigp.color = color;
            uigp.color_bumps = color_bumps;
            uigp.color_hood = color_hood;
            uigp.color_wheels = color_wheels;
            uigp.color_doors = color_doors;

            uigp.skin_body = skin_body;
            uigp.skin_back_bump = skin_back_bump;
            uigp.skin_front_bump = skin_front_bump;
            uigp.skin_left_door = skin_left_door;
            uigp.skin_right_door = skin_right_door;
            uigp.skin_hood = skin_hood;
            uigp.skin_wheel = skin_wheel;

            uigp.profileData = this;

        }
        else
        {
            this.LogError("Failed to retrieve UIGarageProfile OnLoad()");
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
    public List<Keyframe> TORQUE_CURVE;
    [HideInInspector]
    public List<Keyframe> WEIGHT_CURVE;

    // Cosmetics
    [HideInInspector]
    public Color32 color;
    public Color32 color_bumps;
    public Color32 color_hood;
    public Color32 color_wheels;
    public Color32 color_doors;

    public CarColorizable skin_body;
    public CarColorizable skin_front_bump;
    public CarColorizable skin_back_bump;
    public CarColorizable skin_left_door;
    public CarColorizable skin_right_door;
    public CarColorizable skin_hood;
    public CarColorizable skin_wheel;

    // Serizable datas
    public ProfileData profileData;

    UIGarageProfile()
    {

    }

    object ISaveLoad.GetData()
    {
        // TORQUE
        //profileData.ac_torque = new List<SerializableKeyframe>();
        //foreach (Keyframe k in TORQUE_CURVE)
        //{ profileData.ac_torque.Add(k); }

        // WEIGHT
        // profileData.ac_weight = new List<SerializableKeyframe>();
        // foreach (Keyframe k in WEIGHT_CURVE)
        // { profileData.ac_weight.Add(k); }

        // COLOR
        profileData.color = color;
        profileData.color_bumps = color_bumps;
        profileData.color_hood = color_hood;
        profileData.color_wheels = color_wheels;
        profileData.color_doors = color_doors;  

        profileData.skin_body = skin_body;
        profileData.skin_front_bump = skin_front_bump;
        profileData.skin_back_bump = skin_back_bump;
        profileData.skin_hood = skin_hood;
        profileData.skin_wheel = skin_wheel;
        profileData.skin_right_door = skin_right_door;
        profileData.skin_left_door = skin_left_door;

        return profileData;
    }

    void Start()
    {
        //SaveAndLoad.datas.Add(this);
    }
    void OnDestroy()
    {
        //SaveAndLoad.datas.Remove(this);
    }
}
