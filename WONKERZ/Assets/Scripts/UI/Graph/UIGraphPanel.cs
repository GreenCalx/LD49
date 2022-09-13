using UnityEngine;

public class UIGraphPanel : UIGaragePanel
{
    public UIGraphRenderer graphRenderer;
    public UIGraphPoint UIGraphPoint_Ref;
    public GameObject UIGraphCursor_Ref;
    public GameObject UIGraphRange_Ref;
    public float speedInGraphUnit;

    public void show()
    {
        graphRenderer.view.Draw();
        graphRenderer.SetVerticesDirty();
    }

    override public void activate()
    {
        int idx = 0;
        foreach (var pt in graphRenderer.view.points)
        {
            var uiPt = GameObject.Instantiate(UIGraphPoint_Ref, graphRenderer.gameObject.transform);
            uiPt.gameObject.SetActive(true);
            uiPt.transform.localPosition = graphRenderer.view.GraphUnitToDrawUnit(pt);
            tabs.Add(uiPt);
            uiPt.index = idx++;
        }

        // base after tabs creation
        base.activate();

    }

    override public void deactivate() {
        base.deactivate();

        foreach (UITab t in tabs)
        {
            GameObject.Destroy(t.gameObject);
        }
        tabs.Clear();
    }

    public void hideCursor(){
        UIGraphCursor_Ref.SetActive(false);
        UIGraphRange_Ref.SetActive(false);
    }

    public void showCursor()
    {
        UIGraphCursor_Ref.SetActive(true);
        UIGraphRange_Ref.SetActive(true);

        var x = graphRenderer.view.GraphUnitToDrawUnit(graphRenderer.view.points[CurrentTab()]).x;

        var middleX = x;
        var middleSizeX = 0f;
        if (CurrentTab() == 0)
        {
            middleSizeX = (graphRenderer.view.GraphUnitToDrawUnit(graphRenderer.view.points[CurrentTab() + 1]).x - graphRenderer.view.axisBounds.xMin);
            middleX = graphRenderer.view.axisBounds.xMin + middleSizeX / 2;

        }
        else if (CurrentTab() == tabs.Count - 1)
        {
            middleSizeX = (graphRenderer.view.GraphUnitToDrawUnit(graphRenderer.view.points[CurrentTab() - 1]).x - graphRenderer.view.axisBounds.xMax);
            middleX = graphRenderer.view.axisBounds.xMax + middleSizeX / 2;

        }
        else
        {
            middleSizeX = (graphRenderer.view.GraphUnitToDrawUnit(graphRenderer.view.points[CurrentTab() + 1]).x - graphRenderer.view.GraphUnitToDrawUnit(graphRenderer.view.points[CurrentTab() - 1]).x);
            middleX = graphRenderer.view.GraphUnitToDrawUnit(graphRenderer.view.points[CurrentTab() - 1]).x + middleSizeX / 2;
        }


        UIGraphCursor_Ref.transform.localPosition = new Vector3(x, UIGraphCursor_Ref.transform.localPosition.y);
        UIGraphRange_Ref.transform.localPosition = new Vector3(middleX, UIGraphRange_Ref.transform.localPosition.y);
        UIGraphRange_Ref.GetComponent<RectTransform>().sizeDelta =
            new Vector3(middleSizeX, UIGraphRange_Ref.GetComponent<RectTransform>().sizeDelta.y);
    }

    public void moveKey(int idx, float X, float Y)
    {
        var vec = new Vector2(X, Y) * speedInGraphUnit;
        tabs[idx].transform.localPosition += graphRenderer.view.GraphUnitToDrawUnitAbsolute(vec);
        graphRenderer.view.points[idx] += vec;
        show();
        showCursor();
    }

}