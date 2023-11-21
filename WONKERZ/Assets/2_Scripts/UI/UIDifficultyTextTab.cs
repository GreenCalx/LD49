using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Schnibble.UI;

public class UIDifficultyTextTab : UITextTab
{
    public DIFFICULTIES difficulty;

    override public void select()
    {
        UIDifficultyChoice p = Parent as UIDifficultyChoice;
        p.chosen_difficulty = difficulty;
        base.select();
    }

    override public void activate()
    {
        base.activate();

        Debug.Log("Difficulty selected : " + difficulty);
        UIDifficultyChoice p = Parent as UIDifficultyChoice;
        p.chosen_difficulty = difficulty;
        p.choice_made = true;
    }
}
