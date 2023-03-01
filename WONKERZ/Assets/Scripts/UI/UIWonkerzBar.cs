using UnityEngine;
using UnityEngine.UI;
using System;

public class UIWonkerzBar : MonoBehaviour
{
    public Color collected = Color.white;
    public Color missing = Color.black;

    public Image W;
    public Image O;
    public Image N;
    public Image K;
    public Image E;
    public Image R;
    public Image Z;

    private void init()
    {
        CollectiblesManager cm = Access.CollectiblesManager();
        foreach (CollectibleWONKERZ.LETTERS let in Enum.GetValues(typeof(CollectibleWONKERZ.LETTERS)))
        {
            updateLetter(let, cm.hasWONKERZLetter(let));
        }
    }

    public void updateLetter(CollectibleWONKERZ.LETTERS iLetter, bool hasLetter)
    {
        switch (iLetter)
        {
            case CollectibleWONKERZ.LETTERS.W:
                W.color = hasLetter ? collected : missing;
                break;
            case CollectibleWONKERZ.LETTERS.O:
                O.color = hasLetter ? collected : missing;
                break;
            case CollectibleWONKERZ.LETTERS.N:
                N.color = hasLetter ? collected : missing;
                break;
            case CollectibleWONKERZ.LETTERS.K:
                K.color = hasLetter ? collected : missing;
                break;
            case CollectibleWONKERZ.LETTERS.E:
                E.color = hasLetter ? collected : missing;
                break;
            case CollectibleWONKERZ.LETTERS.R:
                R.color = hasLetter ? collected : missing;
                break;
            case CollectibleWONKERZ.LETTERS.Z:
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
