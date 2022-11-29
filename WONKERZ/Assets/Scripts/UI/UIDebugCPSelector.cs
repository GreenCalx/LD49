using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIDebugCPSelector : MonoBehaviour, IControllable
{
    private CheckPointManager cpm;
    private List<string> CPs;
    private List<CheckPoint> eligibleCPs;
    
    private int currSelectedCPIndex;
    private float selectorLatch = 0.2f;
    private float elapsed_time = 0f;
    private bool isActivated = false;

    [Header("MAND")]
    public TextMeshProUGUI TMP_selectedCP;

    public void activate() { isActivated = true; Utils.attachUniqueControllable<UIDebugCPSelector>(this); }
    public void deactivate() { isActivated = false; Utils.detachUniqueControllable(); }


    // Start is called before the first frame update
    void Start()
    {
        elapsed_time = 0f;
        cpm = Access.CheckPointManager();
        CPs = new List<string>();
        eligibleCPs = new List<CheckPoint>();
        foreach(GameObject go in cpm.checkpoints)
        {
            CheckPoint as_cp = go.GetComponent<CheckPoint>();
            if (as_cp.collectMod != CollectiblesManager.COLLECT_MOD.HEAVEN)
                continue;
            CPs.Add(as_cp.checkpoint_name);
            eligibleCPs.Add(as_cp);
        }
        if (CPs.Count <= 0)
        {
            Debug.LogWarning("UIDebugCPSelector::No checkpoints found");
            return;
        }

        currSelectedCPIndex = 0;
        TMP_selectedCP.text = CPs[currSelectedCPIndex];
    }
    
    void IControllable.ProcessInputs(InputManager.InputData Entry)
    {
        if (!isActivated)
            return;

        if (elapsed_time < selectorLatch)
        {
            elapsed_time += Time.unscaledDeltaTime;
            return;
        }

        float X = 0;
        X = Entry.Inputs[Constants.INPUT_UIUPDOWN].AxisValue;
        if (X < -0.2f)
        {
                currSelectedCPIndex = ( currSelectedCPIndex <= 0) ? CPs.Count-1 : currSelectedCPIndex-1;
                elapsed_time = 0f;
                TMP_selectedCP.text = CPs[currSelectedCPIndex];
        }
        else if (X > 0.2f)
        {
                currSelectedCPIndex = ( currSelectedCPIndex >= CPs.Count-1) ? 0 : currSelectedCPIndex+1;
                elapsed_time = 0f;
                TMP_selectedCP.text = CPs[currSelectedCPIndex];
        }

        if (Entry.Inputs[Constants.INPUT_JUMP].IsDown)
        {
            string selectedCP = CPs[currSelectedCPIndex];
            foreach(CheckPoint cp in eligibleCPs)
            {
                if ( cp.checkpoint_name ==  selectedCP)
                {
                    cpm.last_checkpoint = cp.gameObject;
                    cpm.loadLastCP();
                    deactivate();
                    //UIPauseMenu uipm = GetComponentInParent<UIPauseMenu>();
                    //uipm.panel.onDeactivate.Invoke();
                }
            }
        }

        if (Entry.Inputs[Constants.INPUT_CANCEL].IsDown)
        {
            deactivate();
            return;
        }
        elapsed_time += Time.unscaledDeltaTime;
    }
}
