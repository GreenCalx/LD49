using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWonkerzBar : MonoBehaviour
{
    public Color collected  = Color.white;
    public Color missing    = Color.black;

    public Image W;
    public Image O;
    public Image N;
    public Image K;
    public Image E;
    public Image R;
    public Image Z;

    private void init()
    {
        W.color = missing;
        O.color = missing;
        N.color = missing;
        K.color = missing;
        E.color = missing;
        R.color = missing;
        Z.color = missing;
    }

    public void updateLetter( CollectibleWONKERZ.LETTERS iLetter, bool hasLetter)
    {
        switch (iLetter)
        {
            case CollectibleWONKERZ.LETTERS.W :
                W.color = hasLetter ? collected : missing;
                break;
            case CollectibleWONKERZ.LETTERS.O :
                O.color = hasLetter ? collected : missing;
                break;
            case CollectibleWONKERZ.LETTERS.N :
                N.color = hasLetter ? collected : missing;
                break;
            case CollectibleWONKERZ.LETTERS.K :
                K.color = hasLetter ? collected : missing;
                break;
            case CollectibleWONKERZ.LETTERS.E :
                E.color = hasLetter ? collected : missing;
                break;
            case CollectibleWONKERZ.LETTERS.R :
                R.color = hasLetter ? collected : missing;
                break;
            case CollectibleWONKERZ.LETTERS.Z :
                Z.color = hasLetter ? collected : missing;
                break;
            default:
                break;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
