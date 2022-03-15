using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGarageCurve : GarageUISelectable
{

    public enum CAR_PARAM
    {
        UNDEFINED=0,
        TORQUE=1
    };


    public AnimationCurve torque_curve;
   
    private List<Vector2> vertices;

    public UILineRenderer lineRenderer;

    // Start is called before the first frame update
    async void Start()
    {

        vertices = new List<Vector2>(); 

        // Draw animation curve
        float max_time_in_ac = torque_curve[torque_curve.length-1].time;
        for (float i=0f; i<max_time_in_ac; i+=0.1f)
        {
            vertices.Add(new Vector2(i, torque_curve.Evaluate(i)));
        }

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

    public void draw()
    {
        lineRenderer.setPoints(vertices);
    }
}
