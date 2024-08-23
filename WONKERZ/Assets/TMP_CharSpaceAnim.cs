using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


/*
    Animation is in 2 phases :
        1. Expand from charspace = -fontsize (stacked in middle) to fontsize
        2. compress from charspace=fontsize to charSpaceInit
*/

public class TMP_CharSpaceAnim : MonoBehaviour
{


    public TMPro.TextMeshProUGUI target;
    [Header("Autos")]
    public float fontSizeInit = 10;
    public float charSpaceInit = 0;
    [Header("Tweaks")]
    public float animDuration_Expand = 1f;
    public float animDuration_Compress = 1f;

    private float elapsedAnimTime = 0f;
    // Start is called before the first frame update
    void Start()
    {
        if (target==null)
            target = GetComponent<TMPro.TextMeshProUGUI>();
        if (target==null)
        {
            Debug.LogError("No available target for TMP_CHarSpaceAnim. Disabling.");
            enabled = false;
        }
        
        fontSizeInit = target.fontSize;
        charSpaceInit = target.characterSpacing;

        target.characterSpacing = fontSizeInit * -1;
        elapsedAnimTime = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        elapsedAnimTime += Time.deltaTime;
        if (elapsedAnimTime < animDuration_Expand)
        {
            target.characterSpacing = lerp(fontSizeInit * -1, fontSizeInit, elapsedAnimTime/animDuration_Expand);
        }
        else if (elapsedAnimTime < (animDuration_Compress + animDuration_Expand))
        {
            target.characterSpacing = lerp(fontSizeInit, 0f, (animDuration_Expand-elapsedAnimTime)/animDuration_Compress);
        }
        else
        {
            target.characterSpacing = 0f;
            // anim over
        }
    }

    float lerp(float a, float b, float f)
    {
        return a * (1f - f) + (b * f);
    }
}
