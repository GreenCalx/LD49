using UnityEngine;
using System.Collections.Generic;
using Schnibble;
using Schnibble.UI;

namespace Wonkerz {
    public class UIGarageCarStatsPanel : UIGaragePanel
    {
        public GameObject UIGarageCurvePicker_Ref;
        private GameObject UIGarageCurvePicker_Inst;
        public GameObject UICurveMotionRange_Ref;
        private GameObject UICurveMotionRange_Inst;

        public UIGraphPanel graphPanel;

        override protected List<IUIGarageElement.UIGarageHelperValue> getHelperInputs()
        {
            return new List<IUIGarageElement.UIGarageHelperValue>{
                new IUIGarageElement.UIGarageHelperValue(Constants.RES_ICON_A, "OK"),
                new IUIGarageElement.UIGarageHelperValue(Constants.RES_ICON_B, "CANCEL")
            };
        }

        public void selectTorque()
        {
            // update displayed curve
            PlayerController player = (Parent as UIGarage).getGarageEntry().player;

            //graphPanel.graphRenderer.view.SetCurve(player.car_new.car.engine.torqueCurve);

            selectCurve();
        }

        public void selectWeigth()
        {
            // update displayed curve
            PlayerController player = (Parent as UIGarage).getGarageEntry().player;
            //graphPanel.graphRenderer.view.SetCurve(cc.WEIGHT);

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
            #if false
            PlayerController player = (Parent as UIGarage).getGarageEntry().player;
            for (int i = 0; i < graphPanel.graphRenderer.view.points.Length; ++i)
            {
                var v = graphPanel.graphRenderer.view.points[i];
                var k = player.car_new.car.engine.torqueCurve[i];
                k.time = v.x;
                k.value = v.y;
                player.car_new.car.engine.torqueCurve.MoveKey(i, k);
            }
            #endif
            this.Log("player curve updated");
        }

        public void updatePlayerWeightCurve()
        {
            #if false
            CarController cc = (Parent as UIGarage).getGarageEntry().playerCC;

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
}
