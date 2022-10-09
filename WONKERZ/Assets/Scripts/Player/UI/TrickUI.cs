using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrickUI : MonoBehaviour
{
    public TMPro.TextMeshProUGUI TRICKNAME;
    public TMPro.TextMeshProUGUI SCORE;

    public TMPro.TextMeshProUGUI TRICKLINE_SCORE;
    public TMPro.TextMeshProUGUI TRICKLINE_TRICKS;

    public void recordingTrick()
    {
        if (!!TRICKNAME){
            TRICKNAME.color = Color.white;
            var spring = TRICKNAME.GetComponent<SpringMono>();
            if (spring) {
                spring.spring.rest = 0;
            }
        }

    }

    public void validateTrick()
    {
        if (!!TRICKNAME) {
            TRICKNAME.color = Color.green;

            var spring = TRICKNAME.GetComponent<SpringMono>();
            if (spring) {
                spring.spring.rest = 1;
            }

            var animation = TRICKNAME.GetComponent<UIAnimateTransform>();
            if (animation){
                animation.mode = UIAnimateTransform.Mode.SUCCESS;
            }
        }

    }

    public void failTrick()
    {
        if (!!TRICKNAME) {
            TRICKNAME.color = Color.red;
            var spring = TRICKNAME.GetComponent<SpringMono>();
            if (spring) {
                spring.spring.rest = 1;
            }

            var animation = TRICKNAME.GetComponent<UIAnimateTransform>();
            if (animation){
                animation.mode = UIAnimateTransform.Mode.FAIL;
            }
        }
    }

    public void displayTrick( string iTrick )
    {
        if (!!TRICKNAME)
            TRICKNAME.SetText(iTrick);
    }

    public void displayScore( int iScore )
    {
        if ( iScore <= 0 )
            SCORE.SetText("");
        else
            SCORE.SetText(iScore.ToString());
    }

    public void displayTricklineScore( int iScore )
    {
        if ( iScore <= 0 )
            TRICKLINE_SCORE.SetText("");
        else
            TRICKLINE_SCORE.SetText(iScore.ToString());
    }

    public void displayTricklineTricks( List<Trick> iTricks )
    {
        string tricks = "";
        foreach( Trick   t in iTricks )
        {
            tricks += t.name;
            tricks += '\n';
        }
        TRICKLINE_TRICKS.SetText(tricks);
    }
}
