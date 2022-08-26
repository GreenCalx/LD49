using System.Collections.Generic;
using UnityEngine;

public class UIGarageTestManager : MonoBehaviour
{
    public enum MODE { RECORD = 0, REPLAY = 1 };
    public MODE testMode;

    public GameObject startTest;
    private GameObject testCC;
    private InputManager IM;

    private UIGaragePickableTest activeTest;
    private bool isActiveTestReady = false;

    [HideInInspector]
    public float currTestDuration = 0f;

    // To avoid a latch where unity replays physx frames X times to catch up
    private bool replayReadyToStartNextUpdate = false;
    private Queue<InputManager.InputData> replayQueue;
    private int frametowait = 0;

    // Start is called before the first frame update
    void Awake()
    {
        testMode = MODE.REPLAY;
        IM = Access.InputManager();
        activeTest = null;
        replayQueue = new Queue<InputManager.InputData>();
    }

    void FixedUpdate()
    {
        if (frametowait >= 0)
        {
            --frametowait;
        }

        if (frametowait == 0)
        {
            isActiveTestReady = initTest();
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!!activeTest && isActiveTestReady)
        {
            if ((IM.recordedInputs.Count <= 0) && (IM.CurrentMode == InputManager.Mode.AUTOPILOT))
                Access.TestManager().quitTest();

            currTestDuration += Time.deltaTime;
        }
    }

    private bool prepareTest()
    {
        if (testMode == MODE.RECORD)
        {
            // record();
            frametowait = 60;
        }
        else if (testMode == MODE.REPLAY)
        {
            return replay();
        }

        return true;

    }

    private bool initTest()
    {
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
        updateLayers(testCC, Utils.getLayerIndex(Constants.LYR_UIMESH));

        IM.DeActivate();
        if (testMode == MODE.RECORD)
        {
            Utils.attachUniqueControllable(cc);
            record();
        }
        else if (testMode == MODE.REPLAY)
        {
            Utils.attachUniqueControllable(cc);
            IM.startAutoPilot(replayQueue);
        }
        currTestDuration = 0f;
        IM.Activate();

        return true;
    }

    public bool launchTest(UIGaragePickableTest iTest)
    {
        if (testCC != null)
            Destroy(testCC.gameObject);

        activeTest = iTest;

        return prepareTest();
    }

    public void record()
    {
        IM.startRecord();
    }

    public bool replay()
    {
        bool file_loaded = SaveAndLoad.loadGarageTest(activeTest.test_data.test_name, activeTest.test_data);
        if (!file_loaded)
        {
            Debug.LogWarning("Failed to load data.");
            quitTest();
            return false;
        }
        if (!activeTest.test_data.hasData()) // empty record data
        {
            Debug.LogWarning("Not data loaded from the file.");
            quitTest();
            return false;
        }

        replayQueue = new Queue<InputManager.InputData>();
        foreach (SerializableInputData sid in activeTest.test_data.recordData.record)
        { replayQueue.Enqueue(sid); }

        frametowait = 60;

        return true;
    }

    public void updateLayers(GameObject iGO, int iLayer)
    {
        if (iGO == null)
            return;

        iGO.layer = iLayer;
        foreach (Transform child in iGO.transform)
        {
            if (null == child)
                continue;
            updateLayers(child.gameObject, iLayer);
        }
    }

    public void quitTest()
    {
        if (IM.CurrentMode == InputManager.Mode.AUTOPILOT)
        {
            IM.stopAutoPilot();
        }
        if (testCC != null)
        {
            Utils.detachUniqueControllable(testCC.GetComponent<CarController>());
            Destroy(testCC.gameObject);
        }
        activeTest = null;
    }

    public void endTest(bool iSuccess)
    {
        if (iSuccess)
        {
            Debug.Log("TEST OK!");
            if (IM.CurrentMode == InputManager.Mode.RECORD)
            {
                Queue<InputManager.InputData> recorded = IM.stopRecord();
                SaveAndLoad.save(activeTest.test_data.test_name);
            }
        }
        else
        {
            Debug.Log("TEST KO!");
        }
        quitTest();
    }
}
