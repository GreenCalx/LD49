using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerryShockwave : MonoBehaviour
{
    public float     shockwaveDuration = 3f;
    public float     shockwaveSpeed = 2f;
    public Vector3  shockwaveScaleStep;
    public AnimationCurve  YPosOverTime; // [0,1]

    private float lifetimeElapsed = 0f;

    // Start is called before the first frame update
    void Start()
    {
        lifetimeElapsed = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void launch()
    {
        lifetimeElapsed = 0f;
        StartCoroutine(spreadWave(this));
    }

    private IEnumerator spreadWave(TerryShockwave iTS)
    {
        for (float time = 0f; time < shockwaveDuration; time += Time.deltaTime)
        {
            iTS.transform.localScale += shockwaveScaleStep * shockwaveSpeed;
            iTS.transform.position += new Vector3(0, -1 * YPosOverTime.Evaluate(time/shockwaveDuration), 0);
            yield return null;
        }
        Destroy(iTS.gameObject);
    }
}
