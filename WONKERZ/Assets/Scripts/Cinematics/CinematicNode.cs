using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CinematicNode : MonoBehaviour
{
    private float elapsedTime = 0f;
    private bool  nodeActive = false;


    [System.Serializable] public class CineEvent : UnityEvent<float, float, float> {}
    [System.Serializable]
    public struct CinematicStep {
        public CineEvent   func;
        public float       delay;

        public float parm1;
        public float parm2;
        public float parm3;
    }
    public List<CinematicStep> nodeExecs;

    void Awake()
    {
        elapsedTime = 0f;
        nodeActive = false;
    }

    void Update()
    {
        if (nodeActive)
        {
            elapsedTime += Time.deltaTime;
            pollCinematicSteps();
        }
    }

    public void execNode()
    {
        elapsedTime = 0f;
        nodeActive = true;
    }

    private void pollCinematicSteps()
    {
        bool cleanStepsRequired = false;
        foreach( CinematicStep cs in nodeExecs)
        {
            if (cs.delay <= elapsedTime)
            {
                cs.func.Invoke(cs.parm1, cs.parm2, cs.parm3);
                cleanStepsRequired = true;
            }
        }
        if (cleanStepsRequired)
        {
            nodeExecs.RemoveAll( e => (e.delay <= elapsedTime) );
        }
        
    }
}
