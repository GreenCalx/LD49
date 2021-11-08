using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrickUI : MonoBehaviour
{
    public TMPro.TextMeshProUGUI TRICKNAME;
    public TMPro.TextMeshProUGUI SCORE;

    public TMPro.TextMeshProUGUI TRICKLINE_SCORE;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void displayTrick( string iTrick )
    {
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
}
