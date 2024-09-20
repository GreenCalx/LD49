using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble.UI;

namespace Wonkerz {
    public class UISaveLoadTab : UITextTab
    {
        public override void activate()
        {
            base.activate();
        }

        public void save()
        {
            Access.managers.collectiblesMgr.saveJars();
        }

        public void load()
        {
            Access.managers.collectiblesMgr.loadJars();
        }
    }
}
