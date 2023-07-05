using UnityEngine;
using UnityEngine.UI;
using System;

public class UIWonkerzBar : MonoBehaviour
{
    public GameObject showHideHandle;

    public Color collected = Color.white;
    public Color missing = Color.black;
    public float displayTime = 99999f;

    public TMPro.TextMeshProUGUI W;
    public TMPro.TextMeshProUGUI O;
    public TMPro.TextMeshProUGUI N;
    public TMPro.TextMeshProUGUI K;
    public TMPro.TextMeshProUGUI E;
    public TMPro.TextMeshProUGUI R;
    public TMPro.TextMeshProUGUI Z;

    private float elapsedDisplayTime = 0f;
    private bool isDisplayed = false;

    private void init()
    {
        updateAllLetters();
        hide();
    }

    public void updateAllLetters()
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
        if (isDisplayed)
        {
            elapsedDisplayTime += Time.deltaTime;
            if (elapsedDisplayTime >= displayTime)
            {
                hide();
            }
        }
    }

    public void display()
    {
        updateAllLetters();
        isDisplayed = true;
        if (!!showHideHandle)
            showHideHandle.SetActive(true);
        elapsedDisplayTime = 0f;
    }

    public void hide()
    {
        isDisplayed = true;
        if (!!showHideHandle)
            showHideHandle.SetActive(false);        
    }
}
