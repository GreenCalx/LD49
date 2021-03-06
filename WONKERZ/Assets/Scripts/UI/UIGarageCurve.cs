using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UIGarageCurve : MonoBehaviour
{
    [System.Serializable]
    public enum CAR_PARAM
    {
        UNDEFINED=0,
        TORQUE=1,
        WEIGHT=2
    };

    // CURVES
    public AnimationCurve torque_curve;
    public AnimationCurve weight_curve;

    public CAR_PARAM selected_parm;
   
    private List<Vector2> vertices;

    public UILineRenderer lineRenderer;
    public TextMeshProUGUI XLabelRef;
    public TextMeshProUGUI YLabelRef;

    [HideInInspector]
    public float maxbound_nomerge=0f;
    [HideInInspector]
    public float minbound_nomerge=0f;

    // Start is called before the first frame update
    async void Start()
    {
        selected_parm = CAR_PARAM.TORQUE;
        draw();
    }

    public float getValueAtTime(float iTime /*, CAR_PARAM iParm */)
    {
        return torque_curve.Evaluate(iTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void refreshMovableKeyBounds(int iSelectedKeyIndex)
    {
        AnimationCurve ac = getSelectedCurve();
        if (iSelectedKeyIndex <= 0)
        {
            Debug.LogWarning("Moving keyframe at 0 produce weird behaviour. Unable to compute anti merging keybounds.");
            Debug.LogWarning("Cursor slider is frozen.");
            return;
        }
        // Compute min bound
        Keyframe prev_key= ac.keys[iSelectedKeyIndex-1];
        minbound_nomerge = prev_key.time;

        // Compute upper bound
        Keyframe next_key = ac.keys[iSelectedKeyIndex+1];
        maxbound_nomerge = next_key.time;
    }

    public AnimationCurve getSelectedCurve()
    {
        switch( selected_parm )
        {
            case CAR_PARAM.UNDEFINED:
                break;
            case CAR_PARAM.TORQUE:
                return torque_curve;
            case CAR_PARAM.WEIGHT:
                return weight_curve;
            default:
                break;
        }
        return null;
    }

    public void setTorqueCurve(AnimationCurve iAC)
    {
        torque_curve = iAC;
    }

    public void setWeightCurve(AnimationCurve iAC)
    {
        weight_curve = iAC;
    }

    public void setLabels(string XLabel, string YLabel)
    {
        XLabelRef.text = XLabel;
        YLabelRef.text = YLabel;
    }

    public void draw()
    {
        vertices = new List<Vector2>(); 
        AnimationCurve ac = getSelectedCurve();

        // Draw animation curve
        float max_time_in_ac = ac[ac.length-1].time;
        for (float i=0f; i<max_time_in_ac; i+=0.1f)
        {
            vertices.Add(new Vector2(i, ac.Evaluate(i)));
        }

        lineRenderer.refreshFromAnimationCurve(ac);
        lineRenderer.setPoints(vertices);
    }

    public void changeCurve(CAR_PARAM iParm)
    {
        switch( iParm )
        {
            case CAR_PARAM.UNDEFINED:
                break;
            case CAR_PARAM.TORQUE:
                selected_parm = CAR_PARAM.TORQUE;
                break;
            case CAR_PARAM.WEIGHT:
                selected_parm = CAR_PARAM.WEIGHT;
                break;
            default:
                break;
        }
        draw();
    }

    public void moveCurveKey(int iKeyToMove, float iNewTime)
    {
        AnimationCurve selected_curve = getSelectedCurve();
        Keyframe kf = selected_curve[iKeyToMove];
        kf.time = iNewTime;
        selected_curve.MoveKey( iKeyToMove, kf);

        draw();
    }
}
