using UnityEngine;

using Schnibble;

public class UIStartTrack : MonoBehaviour
{
    public TMPro.TextMeshProUGUI countdown_txt;

    // 1D alpha texture.
    // NOTE: must be One dimensionnal byte type.
    public Texture2D opacityRamp;

    public AnimationCurve fontSizeRatioOverTime;

    float initFontSize = 12f;

    void OnValidate() {
        if (opacityRamp.height != 1) {
            this.LogError("opacityRamp should be a 1D texture, height is " + opacityRamp.height);
        }

        if (opacityRamp.format != TextureFormat.R8) {
            this.LogError("opacityRamp should be a single channel byte texture.");
        }

        if (!opacityRamp.isReadable) {
            this.LogError("opacityRamp should be readable.");
        }
    }

    void OnEnable()
    {
        initFontSize = countdown_txt.fontSize;
    }

    void OnDisable() {
        countdown_txt.fontSize = initFontSize;
        countdown_txt.alpha    = 1.0f;
    }

    // Must be called from outside.
    // Caller knows if something happens to update the string, for instance passing from 3 to 2, to 1, etc...
    public void updateDisplay(float iCurrCountdownValue, bool updateString = true)
    {
        if (updateString)
        {
            if (iCurrCountdownValue == 0.0f) countdown_txt.text = "GO!";
            else                             countdown_txt.text = Mathf.CeilToInt(iCurrCountdownValue).ToString();
        }

        float normalizedTime = iCurrCountdownValue % 1.0f;

        countdown_txt.fontSize = initFontSize * fontSizeRatioOverTime.Evaluate(normalizedTime);

        byte newAlpha       = opacityRamp.GetPixelData<byte>(0)[(int)(normalizedTime * opacityRamp.width)];
        countdown_txt.alpha = (float)newAlpha / 255.0f;
    }
}
