using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UIGaragePickableTest : UIGarageSelectable
{
    private UIGarageTestManager uigtm;
    private TextMeshProUGUI txt_elem;
    private bool last_launch_failed;
    [Header("MANDATORY")]
    public UIGarageTestData test_data;
    
    // set by parent
    [HideInInspector]
    public TextMeshProUGUI txt_load_status;

    // Start is called before the first frame update
    void Start()
    {
        if (test_data==null)
        {
            Debug.LogError("No TEST DATA given to a test!!");
            return;
        }
        uigtm = Access.TestManager();
        // set text to test_data name
        txt_elem = GetComponent<TextMeshProUGUI>();
        txt_elem.text = test_data.test_name;

    }

    void Awake()
    {
        txt_load_status.gameObject.SetActive(false);
        last_launch_failed = false;
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
        if (!uigtm.launchTest(this))
        {
            txt_load_status.gameObject.SetActive(true);
            last_launch_failed = true;
            quit();
            return;
        }
        last_launch_failed = false;
        txt_load_status.gameObject.SetActive(false);
    }
    public override void quit() 
    { 
        if (!last_launch_failed) // dont display load error if test launch was a success
            txt_load_status.gameObject.SetActive(false);
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
