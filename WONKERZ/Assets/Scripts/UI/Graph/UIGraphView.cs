using System.Collections.Generic;
using UnityEngine;

public class UIGraphView : Graph
{
    [HideInInspector]
    public List<UIVertex> verts = new List<UIVertex>();
    [HideInInspector]
    public List<int> inds = new List<int>();

    void Update()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        displayBounds = rectTransform.rect;
    }

    UIVertex VecToUIVert(Vector2 pos, Color color)
    {
        UIVertex ret = UIVertex.simpleVert;
        ret.position = GraphUnitToDrawUnit(pos);
        ret.color = color;
        return ret;
    }

    protected override void DrawCurve()
    {
        if (points.Length < 2)
        {
            Debug.LogError("Trying to draw curve with less than two points");
            return;
        }

        var p1 = points[0];
        var p2 = points[1];
        DrawLine(p1.x, p1.y, p2.x, p2.y, curve.color, curve.thickness);

        for (int i = 1; i < points.Length - 1; ++i)
        {
            p1 = points[i];
            p2 = points[i + 1];
            DrawLine(p1.x, p1.y, p2.x, p2.y, curve.color, curve.thickness);

            // add filling triangles
            int idx = verts.Count;
            int idx1 = idx - 8;
            int idx2 = idx - 4;
            inds.Add(idx1 + 1);
            inds.Add(idx2 + 0);
            inds.Add(idx2 + 2);
            inds.Add(idx1 + 3);
            inds.Add(idx1 + 1);
            inds.Add(idx2 + 2);
        }

    }

    public override void DrawLine(float x, float y, float xx, float yy, Color C, float thickness)
    {
        var index = verts.Count;
        //var index = toDraw.currentVertCount;
        // algo tries to make seamless transition by moving point above main one
        Vector2 p1 = new Vector2(x, y);
        Vector2 p2 = new Vector2(xx, yy);
        Vector2 dir = (p2 - p1).normalized;
        Vector2 crossDir = Vector2.Perpendicular(dir) * thickness;
        Vector2 p1temp = p1 + crossDir;
        float offset = 0;
        // if (dir.x != 0f)
        // {
        //     offset = (p1.x - p1temp.x) / dir.x;
        // }
        Vector2 crossDirReflected = offset * dir;

        Vector2 v1 = p1 + crossDir + crossDirReflected;
        Vector2 v2 = p2 + crossDir + crossDirReflected;
        Vector2 v3 = p1 - crossDir - crossDirReflected;
        Vector2 v4 = p2 - crossDir - crossDirReflected;

        verts.Add(VecToUIVert(v1, C));
        verts.Add(VecToUIVert(v2, C));
        verts.Add(VecToUIVert(v3, C));
        verts.Add(VecToUIVert(v4, C));

        inds.Add(index + 0);
        inds.Add(index + 1);
        inds.Add(index + 2);
        inds.Add(index + 1);
        inds.Add(index + 3);
        inds.Add(index + 2);
    }
}
