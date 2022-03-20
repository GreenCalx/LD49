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
   
    private List<Vector2> vertices;

    public UILineRenderer lineRenderer;
    public TextMeshProUGUI XLabelRef;
    public TextMeshProUGUI YLabelRef;

    // Start is called before the first frame update
    async void Start()
    {
        draw(torque_curve);
    }

    public float getValueAtTime(float iTime /*, CAR_PARAM iParm */)
    {
        return torque_curve.Evaluate(iTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setLabels(string XLabel, string YLabel)
    {
        XLabelRef.text = XLabel;
        YLabelRef.text = YLabel;
    }

    public void draw( AnimationCurve iAC)
    {
        vertices = new List<Vector2>(); 

        // Draw animation curve
        float max_time_in_ac = iAC[iAC.length-1].time;
        for (float i=0f; i<max_time_in_ac; i+=0.1f)
        {
            vertices.Add(new Vector2(i, iAC.Evaluate(i)));
        }

        lineRenderer.setPoints(vertices);
    }

    public void changeCurve(CAR_PARAM iParm)
    {
        switch( iParm )
        {
            case CAR_PARAM.UNDEFINED:
                break;
            case CAR_PARAM.TORQUE:
                draw(torque_curve);
                break;
            case CAR_PARAM.WEIGHT:
                draw(weight_curve);
                break;
            default:
                break;
        }
    }
}
