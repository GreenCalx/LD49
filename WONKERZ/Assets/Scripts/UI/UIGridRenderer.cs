using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIGridRenderer : Graphic
{
    public Color cell_color;

    [Delayed]
    public int cellSizeSubDivider = 2;

    public Vector2Int gridSize = new Vector2Int(1,1);
    public float thickness = 10f;

    float cellWidth;
    float cellHeight;
    float width;
    float height;

    public float getWidth()
    { return width; }

    public void setGridSize(Vector2Int iGridSize)
    {
        gridSize = iGridSize;
        SetVerticesDirty();
    }

    private void OnValidate()
    {
        cellSizeSubDivider = (int)Mathf.Pow(2, Mathf.Round(Mathf.Log(cellSizeSubDivider)/Mathf.Log(2)));
    }

    private Vector2[,] subDivisionMatrix(int divFactor)
    {
        if (divFactor<=0)
            return new Vector2[0,0];

        Vector2[,] ret = new Vector2[divFactor,divFactor];

        for(int i=0;i<divFactor;i++)
        {
            for(int j=0;j<divFactor;j++)
            {
                ret[i,j] = new Vector2((cellWidth*i) / divFactor, 
                                        (cellHeight*j)/ divFactor 
                                        );
            }
        }

        return ret;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear(); // clean cache

        RectTransform rectTransform = GetComponent<RectTransform>();
        width = rectTransform.rect.width;
        height = rectTransform.rect.height;
        cellWidth = (width / gridSize.x);
        cellHeight = (height / gridSize.y);

        // grid subdivison
        int divFactor = cellSizeSubDivider;
        Vector2[,] posMatrix = subDivisionMatrix(divFactor);

        // draw
        int count = 0;
        for (int y=0;y<gridSize.y;y++)
        {
            for (int x=0;x<gridSize.x;x++)
            {
                if(posMatrix.Length>0)
                {
                    // draw subdivided
                    for(int i=0;i<posMatrix.GetLength(0);i++)
                    {
                        for (int j=0;j<posMatrix.GetLength(1);j++)
                        {
                            Vector2 pos = posMatrix[i,j];
                            float cx = (x*cellWidth + pos.x);
                            float cy = (y*cellHeight + pos.y);
                            drawCell(cx,cy,cellWidth/divFactor,cellHeight/divFactor,count,vh);
                            count++;
                        }
                    }
                } else {
                    // default draw to curve scale
                    drawCell(x,y,count,vh);
                    count++;
                }

            }
        }


    }

    private void drawCell(float x, float y, float cWidth, float cHeight, int index, VertexHelper vh)
    {
        float xPos = /*cWidth **/ x;
        float yPos =/* cHeight **/ y;

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = cell_color;

        vertex.position = new Vector3(xPos,yPos);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos, yPos + cHeight);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + cWidth, yPos + cHeight);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + cWidth, yPos);
        vh.AddVert(vertex);

        // Connect vertices to draw a square

        float widthSqr = thickness * thickness;
        float distanceSqr = widthSqr / 2f;
        float distance = Mathf.Sqrt(distanceSqr);

        vertex.position = new Vector3(xPos + distance, yPos +distance);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + distance, yPos + (cHeight - distance));
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + (cWidth - distance), yPos + (cHeight - distance) );
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + (cWidth - distance), yPos + distance);
        vh.AddVert(vertex);

        int offset = index * 8;

        // Left
        vh.AddTriangle(offset + 0, offset + 1, offset + 5);
        vh.AddTriangle(offset + 5, offset + 4, offset + 0);

        // Top
        vh.AddTriangle(offset + 1, offset + 2, offset + 6);
        vh.AddTriangle(offset + 6, offset + 5, offset + 1);

        // Right
        vh.AddTriangle(offset + 2, offset + 3, offset + 7);
        vh.AddTriangle(offset + 7, offset + 6, offset + 2);

        // Bottom
        vh.AddTriangle(offset + 3, offset + 0, offset + 4);
        vh.AddTriangle(offset + 4, offset + 7, offset + 3);

    }

    private void drawCell(int x, int y, int index, VertexHelper vh)
    {
        float xPos = cellWidth * x;
        float yPos = cellHeight * y;

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = cell_color;

        vertex.position = new Vector3(xPos,yPos);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos, yPos + cellHeight);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + cellWidth, yPos + cellHeight);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + cellWidth, yPos);
        vh.AddVert(vertex);

        // Connect vertices to draw a square
        //vh.AddTriangle(0,1,2);
        //vh.AddTriangle(2,3,0);

        float widthSqr = thickness * thickness;
        float distanceSqr = widthSqr / 2f;
        float distance = Mathf.Sqrt(distanceSqr);

        vertex.position = new Vector3(xPos + distance, yPos +distance);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + distance, yPos + (cellHeight - distance));
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + (cellWidth - distance), yPos + (cellHeight - distance) );
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + (cellWidth - distance), yPos + distance);
        vh.AddVert(vertex);

        int offset = index * 8;

        // Left
        vh.AddTriangle(offset + 0, offset + 1, offset + 5);
        vh.AddTriangle(offset + 5, offset + 4, offset + 0);

        // Top
        vh.AddTriangle(offset + 1, offset + 2, offset + 6);
        vh.AddTriangle(offset + 6, offset + 5, offset + 1);

        // Right
        vh.AddTriangle(offset + 2, offset + 3, offset + 7);
        vh.AddTriangle(offset + 7, offset + 6, offset + 2);

        // Bottom
        vh.AddTriangle(offset + 3, offset + 0, offset + 4);
        vh.AddTriangle(offset + 4, offset + 7, offset + 3);

    }
}
