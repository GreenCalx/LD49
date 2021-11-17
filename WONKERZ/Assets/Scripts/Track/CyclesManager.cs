using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CyclesManager : MonoBehaviour
{
    [Serializable]
    public class Cycle
    {
        public double start_time;
        public double time;
        public List<DynamicTrackElem> lDTE;

        public Cycle( double iTime, double iStart_offset )
        {
            time = iTime;
            start_time = iStart_offset;
        }
        public bool tryTrigger()
        {
            if ( (Time.time - start_time) >= time )
            {
                foreach( DynamicTrackElem dte in lDTE )
                { dte.trigger(); }
                start_time = Time.time;
                return true;
            }
            return false;
        }
    }

    public double time_step = 0.1;
    [SerializeField]
    public List<Cycle> cycles;
    public bool freeze;

    private double start_time;

    // Start is called before the first frame update
    void Start()
    {
        if ( (cycles == null) || (cycles.Count == 0))
        {
            Debug.LogWarning("CycleManager : No cycles defined.");
            return;
        }

        update_cycles();
        start_time = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (freeze)
            return;

        if ( (Time.time - start_time) >= time_step )
        {
            update_cycles();
            start_time = Time.time;
        }
    }

    private void update_cycles()
    {
        foreach(Cycle c in cycles)
        {
             c.tryTrigger();
        }//! for
    }

}
