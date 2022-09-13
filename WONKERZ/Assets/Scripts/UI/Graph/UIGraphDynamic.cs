using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGraphDynamic : UIPanel
{
    public UIGraphRenderer graphRenderer;

    // Update is called once per frame
    void Update()
    {
        if (isActivated)
        {
            graphRenderer.view.Draw();
            graphRenderer.view.DrawLine(graphRenderer.view.currentValue, graphRenderer.view.axisBounds.yMin, graphRenderer.view.currentValue, graphRenderer.view.axisBounds.yMax, Color.yellow, 0.01f);
            graphRenderer.SetVerticesDirty();
        }
    }
}
