using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Wonkerz;

/**
* Animates a batch of IDENTICAL skinnedmeshrenderers from blend shapes keys
**/
public class BlendShapeLoop : MonoBehaviour
{
    [Header("Renderers")]
    public bool autoFromChildren = true;
    public List<SkinnedMeshRenderer> skinnedMeshRenderers;

    [Header("Tweaks")]
    public bool animate = true;
    public float lerp_step = 0.1f; // the lowest the fastest
    public float animation_speed_mul = 1f;
    public bool randomizeInitFrameIndex = true;

    public int blendShapeCount;

    public Dictionary<SkinnedMeshRenderer, int> renderersToIndex;
    private float elapsedAnimTime;

    // Start is called before the first frame update
    void Start()
    {
        if (autoFromChildren)
        {
            skinnedMeshRenderers = new List<SkinnedMeshRenderer>(GetComponentsInChildren<SkinnedMeshRenderer>());
        }

        if ((skinnedMeshRenderers==null)||(skinnedMeshRenderers.Count==0))
        {
            Debug.LogWarning("BlendShapeLoop : Missing skinned mesh renderers ref");
            gameObject.SetActive(false);
            return;
        }

        blendShapeCount = skinnedMeshRenderers[0].sharedMesh.blendShapeCount;
        renderersToIndex = new Dictionary<SkinnedMeshRenderer, int>();
        foreach(SkinnedMeshRenderer smr in skinnedMeshRenderers)
        {
            int start_index = (randomizeInitFrameIndex)?Random.Range(0,blendShapeCount-1):0;
            renderersToIndex.Add(smr, start_index);
        }

    }

    // Update is called once per frame
    void Update()
    {
        elapsedAnimTime += Time.deltaTime;

        // lerp if > 0
        if (lerp_step>0f)
        {
            float lerp_value = elapsedAnimTime/lerp_step;
            lerp_value = Mathf.Clamp(lerp_value, 0f, 1f);
            // lerp out of previous
            float prev_skey = Utils.lerp(100f,0f,lerp_value);
            float curr_skey = Utils.lerp(0,100f,lerp_value);
            refreshRenderers(prev_skey, curr_skey);
            if (elapsedAnimTime < lerp_step)
                return;
        }

        // move to next animation index
        elapsedAnimTime = 0f;
        refreshRenderers(0f, 100f);

        foreach(SkinnedMeshRenderer smr in skinnedMeshRenderers)
        {
            renderersToIndex[smr]++;
            if (renderersToIndex[smr] >= blendShapeCount)
                renderersToIndex[smr] = 0;
        }
    }

    private void refreshRenderers(float iPrevSKeyVal, float iCurrSKeyVal)
    {
        foreach(SkinnedMeshRenderer smr in skinnedMeshRenderers)
        {
            if (!smr.isVisible)
                continue;

            int playIndex = renderersToIndex[smr];
            
            if (playIndex > 0) 
                smr.SetBlendShapeWeight(playIndex-1, iPrevSKeyVal);
            if (playIndex == 0)
                smr.SetBlendShapeWeight(blendShapeCount-1, iPrevSKeyVal);
            // lerp to next
            smr.SetBlendShapeWeight(playIndex, iCurrSKeyVal);
        }
    }
}
