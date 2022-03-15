using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGarageCurve : GarageUISelectable
{
    public enum curve_mods {
        LINEAR = 0,
        LOG = 1,
        FLAT = 2
    };
    
    public curve_mods slope;
    public curve_mods release_func;


    public float step;
    public int n_points;
    private List<Vector2> vertices;

    public UILineRenderer lineRenderer;

    // Start is called before the first frame update
    void Start()
    {
        vertices = new List<Vector2>();
        computeVertices( 0, n_points, slope);
        computeVertices( n_points, n_points*2, release_func);
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

    public void computeVertices( int iStart, int iEnd, curve_mods iFunc)
    {
        
        for (int i=iStart; i < iEnd; i++)
        {
            vertices.Add(computeFunc(i*step, iFunc));
        }
    }

    public Vector3 computeFunc( float iVertX, curve_mods iFunc )
    {
        Vector3 vert;
        switch (iFunc)
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
            case curve_mods.FLAT:
                vert = new Vector2( iVertX, vertices[vertices.Count - 1].y );
                break;
            default:
                vert = new Vector2(0,0);
                break;
        }
        return vert;
    }
}
