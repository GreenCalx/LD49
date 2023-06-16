using System.Collections.Generic;
using UnityEngine;
using System;
using Schnibble;

public class TSTSaveStates : MonoBehaviour, IControllable
{
    public GameObject saveStateMarkerRef;
    private GameObject saveStateMarkerInst;

    private Vector3 ss_pos = Vector3.zero;
    private Quaternion ss_rot = Quaternion.identity;
    private bool hasSS;

    public Transform startPortal;
    public KeyCode load;
    public KeyCode save;

    private int nPanelUsed;
    private int nPanelRespawn;

    public float ss_latch = 0.2f;
    private float elapsedSinceLastSS = 0f;
    [Serializable]
    public struct ESS
    {
        public KeyCode k;
        public Transform t;
    }
    public List<ESS> ess;

    // Start is called before the first frame update
    void Start()
    {
        hasSS = false;
        nPanelUsed = 0;
        nPanelRespawn = 0;
        elapsedSinceLastSS = 0f;
        Utils.attachControllable(this);
    }

    void OnDestroy()
    {
        Utils.detachControllable(this);
    }

    // Update is called once per frame
    void Update()
    {
        elapsedSinceLastSS += Time.deltaTime;
    }

    void IControllable.ProcessInputs(InputManager.InputData Entry)
    {
        if ( elapsedSinceLastSS < ss_latch)
        {
            return;
        }
        elapsedSinceLastSS = 0f;
        
        var ss_save_or_load = Entry.Inputs[Constants.INPUT_SAVESTATES].AxisValue;
        if (ss_save_or_load > 0) // SAVE
        {
            CheckPointManager cpm = Access.CheckPointManager();
            if (cpm.currPanels<=0)
            {
                return; // no more panels available !
            }
            cpm.currPanels -= 1;

            ss_pos = Access.Player().gameObject.transform.position;
            ss_rot = Access.Player().gameObject.transform.rotation;
            hasSS = true;
            nPanelUsed += 1;
            Access.UITurboAndLifePool().updatePanelUsed(nPanelUsed);
            if (!!saveStateMarkerRef)
            {
                if (!!saveStateMarkerInst)
                    Destroy(saveStateMarkerInst);
                saveStateMarkerInst = Instantiate(saveStateMarkerRef);
                saveStateMarkerInst.transform.position = ss_pos;
                saveStateMarkerInst.transform.rotation = ss_rot;
            }
        }
        else if (ss_save_or_load < 0) // LOAD
        {
            loadState();
        }
    }

    public bool pollExtraSaveStates()
    {
        if (ess == null)
            return false;

        foreach (ESS e in ess)
        {
            if (Input.GetKeyDown(e.k))
            {
                Transform t = e.t;
                if (t == null)
                {
                    this.LogError("No transform could be found for given KeyCode : " + e.k);
                    return false;
                }
                ss_pos = t.position;
                ss_rot = t.rotation;
                return true;
            }
        }
        return false;
    }

    public void loadState()
    {
        if (!hasSS)
        {
            this.LogError("No save state to load. Loading start portal.");
            ss_pos = startPortal.position;
            ss_rot = Quaternion.identity;
            hasSS = true;
        }

        PlayerController player = Access.Player();
        Rigidbody rb2d = player.gameObject.GetComponentInChildren<Rigidbody>();
        if (!!rb2d)
        {
            rb2d.velocity = Vector3.zero;
            rb2d.angularVelocity = Vector3.zero;
        }
        player.gameObject.transform.position = ss_pos;
        player.gameObject.transform.rotation = ss_rot;

        TrickTracker tt = player.GetComponent<TrickTracker>();
        if (!!tt)
        {
            tt.storedScore = 0;
            tt.end_line(true);
        }

        nPanelRespawn += 1;
        Access.UITurboAndLifePool().updatePanelRespawn(nPanelRespawn);
    }
}
