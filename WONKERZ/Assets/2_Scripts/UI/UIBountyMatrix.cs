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
        void Start()
        {
            inputMgr = Access.PlayerInputsManager().player1;

            bountyMatrix = Access.BountyArray();
            activate();
        }

        // Update is called once per frame

        override public void activate()
        {
            bountyMatrix = Access.BountyArray();
            bountyMatrix.initUI(this, tooltip_bountyDesc, tooltip_bountyName, tooltip_bountyReward);
            base.activate();
        }

        override public void deactivate()
        {

            bountyMatrix.hide(this);
            transform.parent.gameObject.SetActive(false);
            base.deactivate();
        }
    }
}
