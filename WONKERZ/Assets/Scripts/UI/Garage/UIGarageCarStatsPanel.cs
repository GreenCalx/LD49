using UnityEngine;
using Schnibble;

public class UIGarageCarStatsPanel : UIGaragePanel
{
    public GameObject UIGarageCurvePicker_Ref;
    private GameObject UIGarageCurvePicker_Inst;
    public GameObject UICurveMotionRange_Ref;
    private GameObject UICurveMotionRange_Inst;

    public UIGraphPanel graphPanel;

    public void selectTorque()
    {
        // update displayed curve
        GameObject player = (Parent as UIGarage).getGarageEntry().playerRef;
        CarController cc = player.GetComponent<CarController>();
        graphPanel.graphRenderer.view.SetCurve(cc.torqueCurve);

        selectCurve();
    }

    public void selectWeigth()
    {
        // update displayed curve
        GameObject player = (Parent as UIGarage).getGarageEntry().playerRef;
        CarController cc = player.GetComponent<CarController>();
        //       graphPanel.graphRenderer.view.SetCurve(cc.WEIGHT);

        selectCurve();
    }

    public void selectCurve()
    {
        // Set Curve to display current stat
        var curr_stat = (GetTab(CurrentTab()) as UIGaragePickableStat);
        // update X/Y Labels of graph
        var graph = graphPanel.graphRenderer.view;
        graph.x.label.text = curr_stat.XLabel;
        graph.y.label.text = curr_stat.YLabel;

        graphPanel.activator = curr_stat;
    }


    public void updatePlayerTorqueCurve()
    {
        GameObject player = (Parent as UIGarage).getGarageEntry().playerRef;
        CarController cc = player.GetComponent<CarController>();
        for (int i = 0; i < graphPanel.graphRenderer.view.points.Length; ++i)
        {
            var v = graphPanel.graphRenderer.view.points[i];
            var k = cc.torqueCurve[i];
            k.time = v.x;
            k.value = v.y;
            cc.torqueCurve.MoveKey(i, k);
        }
        this.Log("player curve updated");
    }

    public void updatePlayerWeightCurve()
    {
#if false
        GameObject player = (Parent as UIGarage).getGarageEntry().playerRef;
        CarController cc = player.GetComponent<CarController>();

        for(int i = 0; i < graphPanel.graphRenderer.view.points.Length; ++i) {
            var v = graphPanel.graphRenderer.view.points[i];
            var k = cc.WEIGHT[i];
            k.time = v.x;
            k.value = v.y;
            cc.WEIGHT.MoveKey(i, k);
        }
        this.Log("player curve updated");
#endif
    }
}
