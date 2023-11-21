using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Schnibble;
using Schnibble.Managers;

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

    public void activate()      { isActivated = true; Access.Player().inputMgr.SetUnique(this as IControllable); }
    public void deactivate()    { isActivated = false; Access.Player().inputMgr.UnsetUnique(this as IControllable); }


    // Start is called before the first frame update
    void Start()
    {
        elapsed_time = 0f;
        cpm = Access.CheckPointManager();
        CPs = new List<string>();
        eligibleCPs = new List<CheckPoint>();
        foreach (GameObject go in cpm.checkpoints)
        {
            CheckPoint as_cp = go.GetComponent<CheckPoint>();
            if (as_cp.collectMod != CollectiblesManager.COLLECT_MOD.HEAVEN)
            continue;
            CPs.Add(as_cp.checkpoint_name);
            eligibleCPs.Add(as_cp);
        }
        if (CPs.Count <= 0)
        {
            this.LogWarn("UIDebugCPSelector::No checkpoints found");
            return;
        }

        currSelectedCPIndex = 0;
        TMP_selectedCP.text = CPs[currSelectedCPIndex];
    }

    void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
    {
        if (!isActivated)
        return;

        if (elapsed_time < selectorLatch)
        {
            elapsed_time += Time.unscaledDeltaTime;
            return;
        }

        var godown = (Entry.Get((int) PlayerInputs.InputCode.UIDown) as GameInputButton).GetState().down;
        var goup = (Entry.Get((int) PlayerInputs.InputCode.UIUp) as GameInputButton).GetState().down;
        if (godown)
        {
            currSelectedCPIndex = (currSelectedCPIndex <= 0) ? CPs.Count - 1 : currSelectedCPIndex - 1;
            elapsed_time = 0f;
            TMP_selectedCP.text = CPs[currSelectedCPIndex];
        }
        else if (goup)
        {
            currSelectedCPIndex = (currSelectedCPIndex >= CPs.Count - 1) ? 0 : currSelectedCPIndex + 1;
            elapsed_time = 0f;
            TMP_selectedCP.text = CPs[currSelectedCPIndex];
        }

        if ((Entry.Get((int) PlayerInputs.InputCode.UIValidate) as GameInputButton).GetState().down)
        {
            string selectedCP = CPs[currSelectedCPIndex];
            foreach (CheckPoint cp in eligibleCPs)
            {
                if (cp.checkpoint_name == selectedCP)
                {
                    cpm.last_checkpoint = cp.gameObject;
                    cpm.loadLastCP();
                    deactivate();
                    break;
                }
            }
        }

        if ((Entry.Get((int) PlayerInputs.InputCode.UICancel) as GameInputButton).GetState().down)
        {
            deactivate();
            return;
        }
        elapsed_time += Time.unscaledDeltaTime;
    }
}
