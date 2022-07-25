using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SerializableInputData
{
    public Dictionary<string, InputManager.InputState> Inputs = new Dictionary<string, InputManager.InputState>();
    public int NumberOfFramesIsSame = 1;
    public bool isDpadDownPressed   = false;
    public bool isDpadUpPressed     = false;
    public bool isDpadLeftPressed   = false;
    public bool isDpadRightPressed  = false;
    public InputManager.InputData buildInputData()
    {
        InputManager.InputData retval = new InputManager.InputData();
        retval.Inputs = Inputs;
        retval.NumberOfFramesIsSame = NumberOfFramesIsSame;
        retval.isDpadDownPressed = isDpadDownPressed;
        retval.isDpadUpPressed = isDpadUpPressed;
        retval.isDpadLeftPressed = isDpadLeftPressed;
        retval.isDpadRightPressed = isDpadRightPressed;
        return retval;
    }

    public InputManager.InputData InputData
    {
        get {
            return buildInputData();
        }
        set {
            Inputs = value.Inputs;
            NumberOfFramesIsSame = value.NumberOfFramesIsSame;
            isDpadDownPressed = value.isDpadDownPressed;
            isDpadUpPressed = value.isDpadUpPressed;
            isDpadLeftPressed = value.isDpadLeftPressed;
            isDpadRightPressed = value.isDpadRightPressed;
        }
    }
    public static implicit operator InputManager.InputData( SerializableInputData inst )
    {
        return inst.InputData;
    }
    public static implicit operator SerializableInputData(InputManager.InputData iInputData)
    {
        return new SerializableInputData{ InputData = iInputData };
    }
}
[System.Serializable]
public class SerializableInputState
{
    public bool IsUp = false;
    public bool IsDown = false;
    public bool Down = false;
    public bool IsAxis = false;
    public float AxisValue = 0f;

    public InputManager.InputState buildInputState()
    {
        InputManager.InputState retval = new InputManager.InputState();
        retval.IsUp = IsUp;
        retval.IsDown = IsDown;
        retval.Down = Down;
        retval.IsAxis = IsAxis;
        retval.AxisValue = AxisValue;
        return retval;
    }

    public InputManager.InputState InputState
    {
        get {
            return buildInputState();
        }
        set {
            IsUp = value.IsUp;
            IsDown = value.IsDown;
            Down = value.Down;
            IsAxis = value.IsAxis;
            AxisValue = value.AxisValue;
        }
    }
    public static implicit operator InputManager.InputState( SerializableInputState inst )
    {
        return inst.InputState;
    }
    public static implicit operator SerializableInputState(InputManager.InputState iInputState)
    {
        return new SerializableInputState{ InputState = iInputState };
    }
}

[System.Serializable]
public class RecordData : EntityData
{
    public Queue<SerializableInputData> record;
    public override void OnLoad(GameObject gameObject) // load in inputmanager
    {
        UIGarageTestData uigpt = gameObject.GetComponent<UIGarageTestData>();
        InputManager im = Utils.GetInputManager();
        if (!!im && !!uigpt)
        {
            im.recordedInputs = new Queue<InputManager.InputData>();
            foreach( SerializableInputData data in record)
            { 
                im.recordedInputs.Enqueue(data);
            }
        } else {
            Debug.LogError("Failed to retrieve InputManager in RecordData.OnLoad()");
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
            return false;
        return recordData.record.Count > 0;
    }

    object ISaveLoad.GetData()
    {
        InputManager im = Utils.GetInputManager();
        recordData.record = new Queue<SerializableInputData>();
        foreach( InputManager.InputData data in im.recordedInputs)
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