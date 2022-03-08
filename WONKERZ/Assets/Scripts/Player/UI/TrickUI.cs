using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrickUI : MonoBehaviour
{
    public TMPro.TextMeshProUGUI TRICKNAME;
    public TMPro.TextMeshProUGUI SCORE;

    public TMPro.TextMeshProUGUI TRICKLINE_SCORE;
    public TMPro.TextMeshProUGUI TRICKLINE_TRICKS;


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void recordingTrick()
    {
        if (!!TRICKNAME)
            TRICKNAME.color = Color.white;
    }

    public void validateTrick()
    {
        if (!!TRICKNAME)
            TRICKNAME.color = Color.green;
    }

    public void failTrick()
    {
        if (!!TRICKNAME)
            TRICKNAME.color = Color.red;
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
