using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStartTrack : MonoBehaviour
{
    public TMPro.TextMeshProUGUI countdown_txt;

    public Texture2D opacityRamp;
    public AnimationCurve fontSizeRatioOverTime;


    private float elapsedTime = 0f;
    private float initFontSize = 12f;
    private string currString = "";

    void Start()
    {
        elapsedTime = 0f;
        initFontSize = countdown_txt.fontSize;
    }

    void Update()
    {
        elapsedTime += Time.fixedDeltaTime;

        float curr_time = Mathf.Min(elapsedTime, 1f);

        countdown_txt.fontSize = fontSizeRatioOverTime.Evaluate(curr_time) * initFontSize;

        float width = opacityRamp.width;
        Color newcolor = opacityRamp.GetPixel((int)(curr_time*width),0);
        countdown_txt.alpha = newcolor.a;
    }


    // Update is called once per frame
    public void updateDisplay(float iCurrCountdownValue)
    {
        if (iCurrCountdownValue < 1f)      {currString = "3";}
        if ((iCurrCountdownValue >= 1f))   {currString = "2";}
        if ((iCurrCountdownValue >= 2f))   {currString = "1";}
        if ((iCurrCountdownValue >= 3f))   {currString = "GO!"; }

        if (currString!=countdown_txt.text)
        { countdown_txt.text = currString; elapsedTime = 0f; }
    }


}
