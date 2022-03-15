using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGarageCurve : GarageUISelectable
{
    public enum curve_mods {
        LINEAR = 0,
        LOG = 1
    };
    public curve_mods func;
    public float step;
    public int n_points;
    private List<Vector2> vertices;

    public UILineRenderer lineRenderer;

    // Start is called before the first frame update
    void Start()
    {
        computeVertices();
        draw();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void draw()
    {
        lineRenderer.setPoints(vertices);
    }

    public void computeVertices()
    {
        vertices = new List<Vector2>(n_points);
        for (int i=0; i < n_points; i++)
        {
            vertices.Add(computeFunc(i*step));
        }
    }

    public Vector3 computeFunc( float iVertX )
    {
        Vector3 vert;
        switch (func)
        {
            case curve_mods.LINEAR:
                vert = new Vector2( iVertX, iVertX);
                break;
            case curve_mods.LOG:
                if (iVertX < 1)
                    vert = new Vector2( iVertX, 0); //truncate [0;1[
                else
                    vert = new Vector2( iVertX, Mathf.Log(iVertX));
                break;
            default:
                vert = new Vector2(0,0);
                break;
        }
        return vert;
    }
}
