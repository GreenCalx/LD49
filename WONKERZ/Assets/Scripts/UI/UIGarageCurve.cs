using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGarageCurve : GarageUISelectable
{
    public enum curve_mods {
        LINEAR = 0,
        LOG = 1
    };
    private LineRenderer lineRenderer;
    public curve_mods func;
    public float step;
    public int n_points;
    private Vector3[] vertices;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = n_points;
        computeVertices();
        draw();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void draw()
    {
        lineRenderer.loop = false; // afaik
        lineRenderer.SetPositions(vertices);
    }

    public void computeVertices()
    {
        vertices = new Vector3[n_points];
        for (int i=0; i < n_points; i++)
        {
            vertices[i] = computeFunc(i*step);
        }
    }

    public Vector3 computeFunc( float iVertX )
    {
        Vector3 vert;
        switch (func)
        {
            case curve_mods.LINEAR:
                vert = new Vector3( iVertX, iVertX, 0);
                break;
            case curve_mods.LOG:
                if (iVertX < 1)
                    vert = new Vector3( iVertX, 0, 0); //truncate [0;1[
                else
                    vert = new Vector3( iVertX, Mathf.Log(iVertX), 0);
                break;
            default:
                vert = new Vector3(0,0,0);
                break;
        }
        return vert;
    }
}
