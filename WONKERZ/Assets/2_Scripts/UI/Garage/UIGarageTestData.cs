using System.Collections.Generic;
using UnityEngine;
using Schnibble;

#if false
[System.Serializable]
public class SerializableInputData
{
    public Dictionary<string, InputManager.InputState> Inputs = new Dictionary<string, InputManager.InputState>();
    public int NumberOfFramesIsSame = 1;
    public bool isDpadDownPressed = false;
    public bool isDpadUpPressed = false;
    public bool isDpadLeftPressed = false;
    public bool isDpadRightPressed = false;
    public InputManager.InputData buildInputData()
    {
        InputManager.InputData retval = new InputManager.InputData();
        // retval.Inputs = Inputs;
        retval.NumberOfFramesIsSame = NumberOfFramesIsSame;
        return retval;
    }

    public InputManager.InputData InputData
    {
        get
        {
            return buildInputData();
        }
        set
        {
            //   Inputs = value.Inputs;
            NumberOfFramesIsSame = value.NumberOfFramesIsSame;
        }
    }
    public static implicit operator InputManager.InputData(SerializableInputData inst)
    {
        return inst.InputData;
    }
    public static implicit operator SerializableInputData(InputManager.InputData iInputData)
    {
        return new SerializableInputData { InputData = iInputData };
    }
}
[System.Serializable]
public class SerializableInputState
{
    public bool IsUp = false;
    public bool as GameInputButton).GetState().down = false;
    public bool Down = false;
    public bool IsAxis = false;
    public float AxisValue = 0f;

    public InputManager.InputState buildInputState()
    {
        InputManager.InputState retval = new InputManager.InputState();
        retval.IsUp = IsUp;
        retval as GameInputButton).GetState().down = as GameInputButton).GetState().down;
        retvalas GameInputButton).GetState().heldDown = Down;
        retval.IsAxis = IsAxis;
        retval as GameInputAxis).GetState().valueRaw = AxisValue;
        return retval;
    }

    public InputManager.InputState InputState
    {
        get
        {
            return buildInputState();
        }
        set
        {
            IsUp = value.IsUp;
            as GameInputButton).GetState().down = value as GameInputButton).GetState().down;
            Down = valueas GameInputButton).GetState().heldDown;
            IsAxis = value.IsAxis;
            AxisValue = value as GameInputAxis).GetState().valueRaw;
}
    }
    public static implicit operator InputManager.InputState(SerializableInputState inst)
    {
        return inst.InputState;
    }
    public static implicit operator SerializableInputState(InputManager.InputState iInputState)
    {
        return new SerializableInputState { InputState = iInputState };
    }
}

[System.Serializable]
public class RecordData : EntityData
{
    public Queue<SerializableInputData> record;
    public override void OnLoad(GameObject gameObject) // load in inputmanager
    {
        UIGarageTestData uigpt = gameObject.GetComponent<UIGarageTestData>();
        InputManager im = Access.InputManager();
        if (!!im && !!uigpt)
        {
            im.recordedInputs = new Queue<InputManager.InputData>();
            foreach (SerializableInputData data in record)
            {
                im.recordedInputs.Enqueue(data);
            }
        }
        else
        {
            this.LogError("Failed to retrieve InputManager in RecordData.OnLoad()");
        }
    }
}

public class UIGarageTestData : MonoBehaviour, ISaveLoad
{
    [Header("MANDATORY")]
    public string test_name;

    // Serizable datas
    public RecordData recordData;

    UIGarageTestData()
    {

    }

    public bool hasData()
    {
        if (recordData.record == null)
        {
            // As we are loading input ino InputManager,
            // if we're loading data without recording it in the same session
            // recordData will be null even though its loaded in the IM
            // thus we refresh datas...
            // TODO : Find a better way to broadcast data loading to every entities?
            InputManager im = Access.InputManager();
            recordData.record = new Queue<SerializableInputData>();
            foreach (InputManager.InputData data in im.recordedInputs)
            {
                recordData.record.Enqueue(data);
            }
        }
        return recordData.record.Count > 0;
    }

    object ISaveLoad.GetData()
    {
        InputManager im = Access.InputManager();
        recordData.record = new Queue<SerializableInputData>();
        foreach (InputManager.InputData data in im.recordedInputs)
        {
            recordData.record.Enqueue(data);
        }

        return recordData;
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

#endif
