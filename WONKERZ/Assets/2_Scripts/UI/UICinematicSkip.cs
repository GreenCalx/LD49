using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICinematicSkip : MonoBehaviour
{

    public Image progressImage;

    // Start is called before the first frame update
    void Start()
    {
       progressImage.fillAmount = 0f; 
    }

    public void updateProgress(float iProgress)
    {
        Debug.Log("update progress " + iProgress);
        float prog = Mathf.Clamp( iProgress, 0f,  1f);
        progressImage.fillAmount = prog;
    }
}
