using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGaragePickableTest : UIGarageSelectable, ISaveLoad
{
    public string test_name;
    public RecordData recordData;
    private UIGarageTestManager uigtm;
    // Start is called before the first frame update
    void Start()
    {
        uigtm = Utils.getTestManager();
    }
    void OnDestroy()
    {
        
    }
    object ISaveLoad.GetData() // fill recordData
    {
        InputManager im = Utils.GetInputManager();
        recordData.record = new Queue<SerializableInputData>();
        foreach( InputManager.InputData data in im.recordedInputs)
        {
            recordData.record.Enqueue(data);
        }

        return recordData;
    }
    // Update is called once per frame
    void Update()
    {

    }
    public override void enter(UIGarageSelector uigs) 
    { 
        base.enter(uigs);

        if(null==uigtm)
        {
            Debug.LogError("Missing UIGarage Test Manager.");
            return;
        }
        SaveAndLoad.datas.Add(this);
        uigtm.launchTest(this);
    }
    public override void quit() 
    { 
        if(null==uigtm)
        {
            Debug.LogError("Missing UIGarage Test Manager.");
        } else {
            uigtm.quitTest();
        }
        SaveAndLoad.datas.Remove(this);
        base.quit(); 
    }


}
