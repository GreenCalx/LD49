using System.Collections.Generic;
using UnityEngine;

public class UIGarageCarStatsPanel : UIGarageCancelablePanel
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
        graphPanel.graphRenderer.view.SetCurve(cc.TORQUE);

        selectCurve();
    }

    public void selectWeigth()
    {
        // update displayed curve
        GameObject player = (Parent as UIGarage).getGarageEntry().playerRef;
        CarController cc = player.GetComponent<CarController>();
        graphPanel.graphRenderer.view.SetCurve(cc.WEIGHT);

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


    public void updatePlayerCurve()
    {
        GameObject player = (Parent as UIGarage).getGarageEntry().playerRef;
        CarController cc = player.GetComponent<CarController>();
        //cc.setCurve(curve.getSelectedCurve(), curve.selected_parm);
        Debug.Log("player curve updated");
    }

    public void open()
    {
        gameObject.SetActive(true);
        animateIn();
        foreach(UITab t in Tabs) {
            t.onDeselect?.Invoke();
        }
    }

    public void close()
    {
        animateOut();
        gameObject.SetActive(false);
    }

}
