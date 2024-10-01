using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using Schnibble.UI;

namespace Wonkerz {
    public class UIGaragePickableColor : UIImageTab
    {
        public List<COLORIZABLE_CAR_PARTS> parts_to_colorize;
        public Material material;

        override public void Activate()
        {
            base.Activate();

            foreach(COLORIZABLE_CAR_PARTS ccp in parts_to_colorize)
            {
                Access.managers.playerCosmeticsMgr.colorize( material , ccp);
            }
        }

        override public void Select()
        {
            base.Select();
            transform.localScale = new Vector3(1.2f, 1.2f, 1.2f );
        }

        override public void Deselect()
        {
            base.Deselect();
            transform.localScale = new Vector3(1f, 1f, 1f );
        }
    }
}
