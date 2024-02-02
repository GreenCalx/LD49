using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;
using Schnibble.Rendering;


public class NPCMouthAnimDecal : WonkerDecal
{
    [Header("# NPCMouthAnimDecal")]
    public float speed;
    public Texture defaultMouth;
    public List<Texture> frames;
    public bool animate = false;
    
    /// -
    private Coroutine animCo;

    public void Animate(bool iState)
    {
        if (animate == iState)
            return;
        //
        animate = iState;
        if (animate)
        {
            if (animCo!=null)
            { StopCoroutine(animCo); animCo = null;}
            animCo = StartCoroutine(MouthAnimCo());
        } 
    }
    

    IEnumerator MouthAnimCo()
    {
        animate = true;
        float internalTimer = 0f;
        float n_frames = frames.Count;
        
        if (n_frames<=0)
            yield break;

        Material toAnimateMat = decalRenderer.decalMat;
        
        float step = 1f / n_frames;
        int displayedIndex  = 0;
        while (animate)
        {
            for (int i=0; i<n_frames ; i += 1)
            {
                float lowerBound = step*i;
                float upperBound = step*(i+1);
                if ((internalTimer >= lowerBound) && (internalTimer < upperBound))
                {
                    if (i != displayedIndex)
                    {
                        displayedIndex = i;
                        toAnimateMat.mainTexture = frames[displayedIndex];
                    }
                    break;
                }
            }

            internalTimer += Time.deltaTime;
            if (internalTimer >= 1f)
                internalTimer = 0f;

            yield return null;
        }
        //  Post 
        toAnimateMat.mainTexture = defaultMouth;
    }

}
