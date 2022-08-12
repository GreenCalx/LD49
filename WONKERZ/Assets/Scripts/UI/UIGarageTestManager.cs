using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGarageTestManager : MonoBehaviour
{
    public enum MODE { RECORD = 0, REPLAY = 1};
    public MODE testMode;

    public GameObject startTest;
    private GameObject testCC;
    private InputManager IM;

    private UIGaragePickableTest activeTest;

    [HideInInspector]
    public float currTestDuration = 0f;

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
        if ((IM.recordedInputs.Count <= 0) && (IM.CurrentMode == InputManager.Mode.AUTOPILOT))
            Utils.getTestManager().quitTest();
        
        if (!!activeTest)
            currTestDuration += Time.deltaTime;
    }

    public bool launchTest(UIGaragePickableTest iTest) 
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

        IM.DeActivate();
        if (testMode == MODE.RECORD)
        {
            Utils.attachUniqueControllable(cc);
            record();
        } 
        else if ( testMode == MODE.REPLAY )
        {
            Utils.attachUniqueControllable(cc);
            if (!replay())
            { IM.Activate(); return false; }
        }
        currTestDuration = 0f;
        IM.Activate();
        
        return true;
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

        Queue<InputManager.InputData> d = new Queue<InputManager.InputData>();
        foreach( SerializableInputData sid in activeTest.test_data.recordData.record)
        { d.Enqueue(sid); }
        IM.startAutoPilot(d);

        return true;
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
        if (IM.CurrentMode == InputManager.Mode.AUTOPILOT)
        {
            IM.stopAutoPilot();
        }
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
                SaveAndLoad.save(activeTest.test_data.test_name);
            }
        } else {
            Debug.Log("TEST KO!");
        }
        quitTest();
    }
}
