using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Schnibble.UI;

namespace Wonkerz
{

    public class UIBountyMatrix : UIPanelTabbed
    {
        private BountyArray bountyMatrix;
        public TextMeshProUGUI tooltip_bountyDesc;
        public TextMeshProUGUI tooltip_bountyName;
        public TextMeshProUGUI tooltip_bountyReward;
        // Start is called before the first frame update
        override protected void Start()
        {
            inputMgr = Access.managers.playerInputsMgr.player1;

            bountyMatrix = Access.managers.bountyArray;
            Activate();
        }

        // Update is called once per frame

        override public void Activate()
        {
            bountyMatrix = Access.managers.bountyArray;
            bountyMatrix.initUI(this, tooltip_bountyDesc, tooltip_bountyName, tooltip_bountyReward);
            base.Activate();
        }

        override public void Deactivate()
        {
            bountyMatrix.hide(this);
            transform.parent.gameObject.SetActive(false);
            base.Deactivate();
        }
    }
}
