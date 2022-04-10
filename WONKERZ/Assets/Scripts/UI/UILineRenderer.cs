using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILineRenderer : Graphic
{
    [HideInInspector]
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
                gridRenderer.setGridSize(gridSize);
                //SetVerticesDirty(); // invalidate
            }
        }
    }

    public void refreshFromAnimationCurve( AnimationCurve iAC )
    {
        Keyframe[] keys  = iAC.keys;
        int n_keys = keys.Length;

        float min_time=0f;
        float max_time=0f;
        float min_val=0f;
        float max_val=0f;
        for (int i=0;i<n_keys;i++)
        {
            Keyframe selected = keys[i];
            if (i==0)
            {
                min_time = selected.time;
                max_time = selected.time;
                min_val = selected.value;
                max_val = selected.value;
                continue;
            }
            // X bounds
            if ( min_time > selected.time )
                min_time = selected.time;
            if ( max_time < selected.time )
                max_time = selected.time;

            // Y bounds
            if ( min_val > selected.value )
                min_val = selected.value;
            if ( max_val < selected.value )
                max_val = selected.value;
        }

        // refresh sizes
        int sizex = (int)Mathf.Ceil(max_time - min_time);
        int sizey = (int)Mathf.Ceil(max_val - min_val);
        if (sizex < 1)
        {
            sizex = 1;
            Debug.LogWarning("size x < 1 !! Resized to 1.");
        }
        if (sizey < 1)
        {
            sizey = 1;
            Debug.LogWarning("size y < 1 !! Resized to 1.");
        }
        gridSize = new Vector2Int(sizex,sizey+1);
        
        SetVerticesDirty();
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
