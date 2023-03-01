using UnityEngine;
using TMPro;
using Schnibble;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UIGaragePickableTest : UITextTab
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
        if (test_data == null)
        {
            this.LogError("No TEST DATA given to a test!!");
            return;
        }
        uigtm = Access.TestManager();
        // set text to test_data name
        txt_elem = GetComponent<TextMeshProUGUI>();
        txt_elem.text = test_data.test_name;
    }

    protected override void Awake()
    {
        base.Awake();
        txt_load_status.gameObject.SetActive(false);
        last_launch_failed = false;
    }

    protected override void ProcessInputs(InputManager.InputData Entry)
    {
        base.ProcessInputs(Entry);

        if (Entry.Inputs[Constants.INPUT_RESPAWN].IsDown)
        {
            setModeRecord();
            activate();
        }
    }

    public void setModeReplay()
    {
        Access.TestManager().testMode = UIGarageTestManager.MODE.REPLAY;
    }

    public void setModeRecord()
    {
        Access.TestManager().testMode = UIGarageTestManager.MODE.RECORD;
    }

    override public void activate()
    {
        base.activate();

        SaveAndLoad.datas.Add(test_data);
        if (!uigtm.launchTest(this))
        {
            txt_load_status.gameObject.SetActive(true);
            last_launch_failed = true;
            deactivate();
            return;
        }
        last_launch_failed = false;
        txt_load_status.gameObject.SetActive(false);
    }
    override public void deactivate()
    {
        base.deactivate();

        if (!last_launch_failed) // dont display load error if test launch was a success
            txt_load_status.gameObject.SetActive(false);
        SaveAndLoad.datas.Remove(test_data);
    }


}
