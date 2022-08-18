using System;
using System.Collections.Generic;
using UnityEngine;

public interface IControllable
{
    void ProcessInputs(InputManager.InputData Entry);
}

public enum Joystick
{
    A = 1000,
    B,
    X,
    Y,
    LT,
    RT,
    LS,
    RS,
    LB,
    RB,
    DpadV,
    DpadH,
    LeftV,
    LeftH,
    RightV,
    RightH,
    Select,
    Start
}

static public class InputSettings
{
    public static bool InverseRSMapping = false;
    public static float MouseMultiplier = 20;
    public struct InputMappingData
    {
        public InputMappingData(bool A, bool B, List<int> C, List<int> D)
        {
            IsAxis = A;
            IsMouse = B;
            Positive = C;
            Negative = D;
        }
        public bool IsAxis;
        public bool IsMouse;
        public List<int> Positive;
        public List<int> Negative;
    }
    // IMPORTANT toffa : joystick value first, then keyboard for now!!!!
    static public Dictionary<String, InputMappingData> Mapping = new Dictionary<String, InputMappingData>{
        {"Accelerator", new InputMappingData( true, false, new List<int>{(int)Joystick.RT, (int)KeyCode.W}, new List<int>{ (int)Joystick.LT, (int)KeyCode.S} )},
        {"UIUpDown", new InputMappingData( true, false, new List<int>{(int)Joystick.LeftH, (int)KeyCode.W}, new List<int>{ (int)Joystick.LeftH, (int)KeyCode.S} )},
        {"Turn" , new InputMappingData(true, false, new List<int>{(int)Joystick.LeftH, (int)KeyCode.D}, new List<int>{-1, (int)KeyCode.A})},
        {"Jump", new InputMappingData(false, false, new List<int>{ (int) Joystick.A, (int)KeyCode.Space }, new List<int>{-1,-1} )},
        {"Respawn", new InputMappingData(false, false, new List<int>{ (int) Joystick.Start, (int)KeyCode.R }, new List<int>{-1,-1} )},
        {"CameraX" , new InputMappingData(true, true, new List<int>{ (int) (InverseRSMapping ? Joystick.RightH : Joystick.RightV) }, new List<int>{-1})},
        {"CameraY" , new InputMappingData(true, true, new List<int>{ (int) (InverseRSMapping ? Joystick.RightV : Joystick.RightH) }, new List<int>{-1})},
        {"Grapin", new InputMappingData(false, false, new List<int>{ (int) Joystick.X, (int)KeyCode.F }, new List<int>{-1,-1} )},
        {"Cancel", new InputMappingData(false, false, new List<int>{ (int) Joystick.B, (int)KeyCode.Escape }, new List<int>{-1,-1} )},
    };
}

public class InputManager : MonoBehaviour
{
    [System.Serializable]
    public class InputState
    {
        public bool IsUp = false;
        public bool IsDown = false;
        public bool Down = false;
        public bool IsAxis = false;
        public float AxisValue = 0f;
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                InputState B = (InputState)obj;
                return (IsUp == B.IsUp && IsDown == B.IsDown && Down == B.Down && IsAxis == B.IsAxis && AxisValue == B.AxisValue);
            }
        }
    }
    [System.Serializable]
    public class InputData
    {
        public Dictionary<String, InputState> Inputs = new Dictionary<string, InputState>();
        public int NumberOfFramesIsSame = 1;
        public bool isDpadDownPressed = false;
        public bool isDpadUpPressed = false;
        public bool isDpadLeftPressed = false;
        public bool isDpadRightPressed = false;
        public void Add(String s)
        {
            var Mapping = InputSettings.Mapping[s];
            InputState I = new InputState();

            if (Mapping.IsAxis)
            {
                I.IsAxis = true;
                if (Mapping.IsMouse)
                {
                    if (s.Contains("X"))
                    {
                        I.AxisValue = ((_LastMousePosition.x - Input.mousePosition.x) / Screen.width) *InputSettings.MouseMultiplier;
                    }
                    else
                    {
                        I.AxisValue = ((_LastMousePosition.y - Input.mousePosition.y) / Screen.height) *InputSettings.MouseMultiplier;
                    }
                    I.AxisValue += Input.GetAxisRaw( "Joy" + ((Joystick)Mapping.Positive[0]).ToString() );
                }
                else
                {
                    I.AxisValue = Input.GetAxisRaw("Joy" + ((Joystick)(Mapping.Positive[0])).ToString());
                    I.AxisValue -= Mapping.Negative[0] != -1 ? Input.GetAxisRaw("Joy" + ((Joystick)(Mapping.Negative[0])).ToString()) : 0;
                    I.AxisValue += Input.GetKey((KeyCode)(Mapping.Positive[1])) ? 1 : 0;
                    I.AxisValue -= Input.GetKey((KeyCode)(Mapping.Negative[1])) ? 1 : 0;
                }
            }
            else
            {
                I.IsUp = Input.GetButtonUp("Joy" + ((Joystick)(Mapping.Positive[0])).ToString()) || Input.GetKeyUp((KeyCode)(Mapping.Positive[1]));
                I.IsDown = Input.GetButtonDown("Joy" + ((Joystick)(Mapping.Positive[0])).ToString()) || Input.GetKeyDown((KeyCode)(Mapping.Positive[1]));
                I.Down = Input.GetButton("Joy" + ((Joystick)(Mapping.Positive[0])).ToString()) || Input.GetKey((KeyCode)(Mapping.Positive[1])); ;
            }
            Inputs.Add(s, I);
        }
        public override bool Equals(System.Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                InputData B = (InputData)obj;

                foreach (KeyValuePair<String, InputState> Value in Inputs)
                {
                    if (!B.Inputs[Value.Key].Equals(Value.Value))
                    {
                        return false;
                    }
                }
                if (isDpadDownPressed != B.isDpadDownPressed || isDpadLeftPressed != B.isDpadLeftPressed ||
                        isDpadRightPressed != B.isDpadRightPressed || isDpadUpPressed != B.isDpadUpPressed)
                    return false;
                return true;
            }
        }
    }

    private List<IControllable> _Controllees = new List<IControllable>();
    private float _LastDpadAxisHorizontal = 0;
    private float _LastDpadAxisVertical = 0;
    private static Vector2 _LastMousePosition;

    public enum Mode {  PLAYER, // default mode to control stuff
                        DEACTIVATED, 
                        RECORD, // Like Player but records inputs until stopRecord() is called
                        AUTOPILOT // Pushes recordedInputs into Entry instead of real inputs
                        };
    public Mode CurrentMode = Mode.DEACTIVATED;

    private bool _Lock = false;
    private List<IControllable> _DeferRemove = new List<IControllable>();
    public bool _Activated = true;

    private Stack<IControllable> _PriorityList = new Stack<IControllable>();

    public Queue<InputData> recordedInputs = new Queue<InputData>();

    private bool frameLock; // for replay consistency
    private int nLockedFrames;


    public void Activate() { _Activated = true; }
    public void DeActivate() { _Activated = false; }

    public void Attach(IControllable iControllable)
    {
        if (!_Controllees.Contains(iControllable))
        {
            _Controllees.Add(iControllable);
        }
    }

    public void Detach(IControllable iControllable)
    {
        if (!_Lock)
            _Controllees.Remove(iControllable);
        else
            _DeferRemove.Add(iControllable);

    }

    public void DetachAll()
    {
        if (!_Lock)
            _Controllees.Clear();
        else
        {
            foreach(IControllable controllable in _Controllees)
            {
                _DeferRemove.Add(controllable);
            }
        }
    }

    public void SetUnique(IControllable iControllable)
    {
        _PriorityList.Push(iControllable);
    }

    public void UnsetUnique(IControllable iControllable)
    {
        if (iControllable != _PriorityList.Peek())
        {
            Debug.LogError("Trying to unstack while not the first item");
            return;
        }

        _PriorityList.Pop();
    }

    private void Lock()
    {
        _Lock = true;
    }

    private void UnLock()
    {
        _Lock = false;
        foreach (IControllable C in _DeferRemove)
        {
            Detach(C);
        }
        _DeferRemove.Clear();
    }

    public void startRecord()
    {
        recordedInputs.Clear();
        if (Constants.DBG_REPLAYDUMP)
        { GarageTestDump.initStack(); }
        CurrentMode = Mode.RECORD;
    }

    public Queue<InputData> stopRecord()
    {
        CurrentMode = Mode.PLAYER;
        if (Constants.DBG_REPLAYDUMP)
        { GarageTestDump.dumpStack("last_test_record.txt"); }
        return recordedInputs;
    }

    public void startAutoPilot(Queue<InputData> iDatas)
    {
        CurrentMode = Mode.AUTOPILOT;
        recordedInputs = iDatas;
        nLockedFrames = 0;
        if (Constants.DBG_REPLAYDUMP)
        { GarageTestDump.initStack(); }
    }
    public void stopAutoPilot()
    {
        if (recordedInputs.Count == 0 )
        { Debug.Log("All recorded inputs have been processed.");}
        else { Debug.Log( recordedInputs.Count +" Inputs unplayed."); }
        CurrentMode = Mode.PLAYER;
        Debug.Log("Number of frames locked by physics : " + nLockedFrames);
        nLockedFrames = 0;
        if (Constants.DBG_REPLAYDUMP)
        { GarageTestDump.dumpStack("last_test_replay.txt"); }
    }

    void FixedUpdate()
    {
        frameLock = false; // physx tick
    }

    void Update()
    {
        if (!_Activated) return;
        // NOTE(toffa): Saver stuff test
        InputData Entry = new InputData();

        if (CurrentMode == Mode.AUTOPILOT )
        {
            if (recordedInputs.Count > 0 )
            {
                Entry = recordedInputs.Dequeue();
                if (Constants.DBG_REPLAYDUMP)
                { GarageTestDump.addToStack(Access.TestManager().currTestDuration, Entry); }
                if (frameLock)
                { nLockedFrames++; return; }
                frameLock = true;
            }
        } else {
            foreach (string K in InputSettings.Mapping.Keys)
            {
                Entry.Add(K);
            }
        }
        if (CurrentMode == Mode.RECORD)
        {
            recordedInputs.Enqueue(Entry);
            if (Constants.DBG_REPLAYDUMP)
            { GarageTestDump.addToStack(Access.TestManager().currTestDuration, Entry); }
        }

        //_LastDpadAxisHorizontal = Entry.Inputs["DPad_Horizontal"].AxisValue;
        //_LastDpadAxisVertical = Entry.Inputs["DPad_Vertical"].AxisValue;

        // Set mode according to inputs

        // IMPORTANT toffa : the collection can change as process inputs could attach or detach gameobjects
        // so we must use vanilla for and not foreach
        // we could also copy the collection before calling process inputs, but I guess we want to processInputs
        // of newly attached object in this frame and not the next one
        var EndIdx = _Controllees.Count;
        Lock();
        if (_PriorityList.Count != 0)
        {
            _PriorityList.Peek().ProcessInputs(Entry);
        }
        else
        {
            for (int i = 0; i < _Controllees.Count; ++i)
            {
                _Controllees[i].ProcessInputs(Entry);
            }
        }
        UnLock();

        _LastMousePosition = Input.mousePosition;
    }

}
