using System.Collections.Generic;
using UnityEngine;
using Schnibble;

[System.Serializable]
public class SerializableColor
{
    public float[] colorStore = new float[4] { 1F, 1F, 1F, 1F };
    public Color Color
    {
        get { return new Color(colorStore[0], colorStore[1], colorStore[2], colorStore[3]); }
        set { colorStore = new float[4] { value.r, value.g, value.b, value.a }; }
    }
    public static implicit operator Color(SerializableColor inst)
    {
        return inst.Color;
    }
    public static implicit operator SerializableColor(Color iColor)
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
    public SerializableColor color;
    public override void OnLoad(GameObject gameObject)
    {
        UIGarageProfile uigp = gameObject.GetComponent<UIGarageProfile>();
        if (!!uigp)
        {
            // TORQUE
            uigp.TORQUE_CURVE = new List<Keyframe>();
            foreach (SerializableKeyframe sk in ac_torque)
            { uigp.TORQUE_CURVE.Add(sk); }

            // WEIGHT
            uigp.WEIGHT_CURVE = new List<Keyframe>();
            foreach (SerializableKeyframe sk in ac_weight)
            { uigp.WEIGHT_CURVE.Add(sk); }

            // COLOR
            uigp.color = color;

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
    public Color color;

    // Serizable datas
    public ProfileData profileData;

    UIGarageProfile()
    {

    }

    object ISaveLoad.GetData()
    {
        // TORQUE
        profileData.ac_torque = new List<SerializableKeyframe>();
        foreach (Keyframe k in TORQUE_CURVE)
        { profileData.ac_torque.Add(k); }

        // WEIGHT
        profileData.ac_weight = new List<SerializableKeyframe>();
        foreach (Keyframe k in WEIGHT_CURVE)
        { profileData.ac_weight.Add(k); }

        // COLOR
        profileData.color = color;


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
