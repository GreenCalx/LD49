using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        UIGaragePickableTest uigpt = gameObject.GetComponent<UIGaragePickableTest>();
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

public class UIGarageTestManager : MonoBehaviour
{
    public enum MODE { RECORD = 0, REPLAY = 1};
    public MODE testMode;

    public GameObject startTest;
    private GameObject testCC;
    private InputManager IM;

    private UIGaragePickableTest activeTest;

    // Start is called before the first frame update
    void Awake()
    {
        testMode = MODE.REPLAY;
        IM = Utils.GetInputManager();
        activeTest = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void launchTest(UIGaragePickableTest iTest)
    {
        if (testCC!=null)
            Destroy(testCC.gameObject);

        activeTest = iTest;

        // clone player to start test position
        testCC = Instantiate(Utils.getPlayerRef(), transform);
        UIGarageTestStart uigts = startTest.GetComponent<UIGarageTestStart>();
        
        CarController cc = testCC.GetComponent<CarController>();
        cc.isFrozen = false;

        testCC.transform.position = uigts.respawnPoint.position;
        testCC.transform.rotation = uigts.respawnPoint.rotation;
        Rigidbody rb2d = testCC.GetComponentInChildren<Rigidbody>();
        if (!!rb2d)
        {
            rb2d.velocity = Vector3.zero;
            rb2d.angularVelocity = Vector3.zero;
        }
        updateLayers( testCC, Utils.getLayerIndex(Constants.LYR_UIMESH));


        if (testMode == MODE.RECORD)
        {
            record();
            Utils.attachUniqueControllable(cc);
        } 
        else if ( testMode == MODE.REPLAY )
        {
            replay();
            Utils.attachUniqueControllable(cc);
        }

    
        // switch input manager to automod

        // launch simulation until it reaches end position
        
    }

    public void record()
    {
        IM.startRecord();
    }

    public void replay()
    {
        //IM.startAutoPilot(/*Deserialized datas*/);
        SaveAndLoad.load(activeTest.test_name);
    }

    public void updateLayers(GameObject iGO, int iLayer)
    {
        if (iGO==null)
            return;

        iGO.layer = iLayer;
        foreach( Transform child in iGO.transform )
        {
            if (null==child)
                continue;
            updateLayers(child.gameObject, iLayer);
        }
    }

    public void quitTest()
    {
        if (testCC!=null)
        {
            Utils.detachUniqueControllable(testCC.GetComponent<CarController>());
            Destroy(testCC.gameObject);
        }
        activeTest = null;
    }

    public void endTest( bool iSuccess)
    {
        if (iSuccess)
        {
            Debug.Log("TEST OK!");
            if (IM.CurrentMode == InputManager.Mode.RECORD)
            {
                Queue<InputManager.InputData> recorded = IM.stopRecord();
                // TODO : Serialize here
                SaveAndLoad.save(activeTest.test_name);
            }
        } else {
            Debug.Log("TEST KO!");
        }
        quitTest();
    }
}
