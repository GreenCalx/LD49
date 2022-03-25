using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILineRenderer : Graphic
{
    public Vector2Int gridSize;
    public List<Vector2> points;

    float width;
    float height;
    public float unitWidth;
    public float unitHeight;

    public float thickness = 10f;

    public UIGridRenderer gridRenderer ;

    void Start()
    {
        if (gridRenderer==null)
            gridRenderer = GetComponentInParent<UIGridRenderer>();
    }

    void Update()
    {
        if (gridRenderer!=null)
        {
            if(gridSize != gridRenderer.gridSize)
            {
                gridSize = gridRenderer.gridSize;
                SetVerticesDirty(); // invalidate
            }
        }
    }

    public void setPoints(List<Vector2> iPoints)
    {
        points = iPoints;
        SetVerticesDirty(); // invalidate
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        RectTransform rectTransform = GetComponent<RectTransform>();
        width = rectTransform.rect.width;
        height = rectTransform.rect.height;

        unitWidth = width / (float)gridSize.x;
        unitHeight = height / (float)gridSize.y;

        if (points.Count < 2)
        { return; }

        float angle = 0f;
        for (int i=0; i < points.Count; i++)
        {
            Vector2 point = points[i];
            if (i<points.Count - 1)
            {
                angle = GetAngle( points[i], points[i+1]) + 45f;
            }

            drawVerticesForPoint( point, vh, angle);
        }

        for (int j=0;j<points.Count-1;j++)
        {
            int index = j*2; // 2 vert
            vh.AddTriangle( index + 0, index + 1, index + 3);
            vh.AddTriangle( index + 3, index + 2, index + 0);
        }

    }

    public float GetAngle(Vector2 caller, Vector2 target)
    {
        return (float)(Mathf.Atan2(target.y - caller.y, target.x - caller.x) * (180/Mathf.PI));
    }

    private void drawVerticesForPoint( Vector2 point, VertexHelper vh, float angle)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        vertex.position = Quaternion.Euler(0,0,angle) * new Vector3(-thickness/2, 0);
        vertex.position += new Vector3( unitWidth * point.x, unitHeight * point.y);
        vh.AddVert(vertex);

        vertex.position = Quaternion.Euler(0,0,angle) * new Vector3(thickness/2, 0);
        vertex.position += new Vector3( unitWidth * point.x, unitHeight * point.y);
        vh.AddVert(vertex);

    }
}
