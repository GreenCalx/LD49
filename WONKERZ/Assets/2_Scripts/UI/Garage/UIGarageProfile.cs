using System.Collections.Generic;
using UnityEngine;
using Schnibble;

[System.Serializable]
public class SerializableSkin
{
    public int carPartID; // CarPartToColorize
    public int partSkinID;  // id of the skin for a given car part
    public int materialSkinID; // id of the material for a given car part

    public CarColorizable buildSkin()
    {
        CarColorizable retval = new CarColorizable();
        retval.part = (COLORIZABLE_CAR_PARTS) carPartID;
        retval.partSkinID = partSkinID;
        retval.materialSkinID = materialSkinID;
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
            partSkinID = value.partSkinID;
            materialSkinID = value.materialSkinID;
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

            // COSMETICS
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
