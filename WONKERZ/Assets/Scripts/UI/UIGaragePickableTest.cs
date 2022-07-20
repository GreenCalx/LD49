using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UIGaragePickableTest : UIGarageSelectable
{
    private UIGarageTestManager uigtm;
    [Header("MANDATORY")]
    public UIGarageTestData test_data;
    private TextMeshProUGUI txt_elem;
    // Start is called before the first frame update
    void Start()
    {
        if (test_data==null)
        {
            Debug.LogError("No TEST DATA given to a test!!");
            return;
        }
        uigtm = Utils.getTestManager();
        // set text to test_data name
        txt_elem = GetComponent<TextMeshProUGUI>();
        txt_elem.text = test_data.test_name;
    }
    void OnDestroy()
    {
        
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
        SaveAndLoad.datas.Add(test_data);
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
        SaveAndLoad.datas.Remove(test_data);
        base.quit(); 
    }


}
