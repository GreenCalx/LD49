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

        IM.DeActivate();
        if (testMode == MODE.RECORD)
        {
            Utils.attachUniqueControllable(cc);
            record();
        } 
        else if ( testMode == MODE.REPLAY )
        {
            Utils.attachUniqueControllable(cc);
            replay();
        }
        IM.Activate();
    
        // launch simulation until it reaches end position
        
    }

    public void record()
    {
        IM.startRecord();
    }

    public void replay()
    {
        bool file_loaded = SaveAndLoad.loadGarageTest(activeTest.test_data.test_name, activeTest.test_data);
        if (!file_loaded)
        {
            quitTest();
            return;
        }

        Queue<InputManager.InputData> d = new Queue<InputManager.InputData>();
        foreach( SerializableInputData sid in activeTest.test_data.recordData.record)
        { d.Enqueue(sid); }
        IM.startAutoPilot(d);
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
        IM.stopAutoPilot();
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
                SaveAndLoad.save(activeTest.test_data.test_name);
            }
        } else {
            Debug.Log("TEST KO!");
        }
        quitTest();
    }
}
