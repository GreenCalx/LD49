using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Schnibble;
using Schnibble.UI;

namespace Wonkerz {

    public class UIDifficultyTextTab : UITextTab
    {
        public DIFFICULTIES difficulty;

        override public void Select()
        {
            UIDifficultyChoice p = parent as UIDifficultyChoice;
            p.chosen_difficulty = difficulty;
            base.Select();
        }

        override public void Activate()
        {
            base.Activate();

            this.Log("Difficulty selected : " + difficulty);
            UIDifficultyChoice p = parent as UIDifficultyChoice;
            p.chosen_difficulty = difficulty;
            p.choice_made = true;
        }
    }
}
