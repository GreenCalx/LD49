using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStartTrack : MonoBehaviour
{
    public TMPro.TextMeshProUGUI countdown_txt;

    // 1D alpha texture.
    public Texture2D opacityRamp;

    public AnimationCurve fontSizeRatioOverTime;

    private float initFontSize = 12f;

    void Start()
    {
        initFontSize = countdown_txt.fontSize;
    }

    // Must be called from outside.
    // Caller knows if something happens to update the string, for instance passing from 3 to 2, to 1, etc...
    public void updateDisplay(float iCurrCountdownValue, bool updateString = true)
    {
        if (updateString)
        {
            if (iCurrCountdownValue == 0.0f)
            {
                countdown_txt.text = "GO!";
            }
            else
            {
                countdown_txt.text = Mathf.CeilToInt(iCurrCountdownValue).ToString();
            }
        }

        float normalizedTime = iCurrCountdownValue % 1.0f;

        countdown_txt.fontSize = fontSizeRatioOverTime.Evaluate(normalizedTime) * initFontSize;

        float newAlpha = opacityRamp.GetPixelData<float>(0)[(int)(normalizedTime * opacityRamp.width)];
        countdown_txt.alpha = newAlpha;

    }
}
