using UnityEngine;
using Schnibble;

public class TorchAnimation : MonoBehaviour
{
    public AnimationCurve AC;
    public Light target_light;
    private float elapsed_time;
    private float base_intensity;

    private bool rewind_read = false;

    private Keyframe firstFrame;
    private Keyframe lastFrame;

    // Start is called before the first frame update
    void Start()
    {
        elapsed_time = 0f;
        base_intensity = target_light.intensity;

        int n_keys = AC.length;
        if (n_keys < 2)
        {
            this.Log("Not enough keyframes (2 min) for torchanomation");
            this.enabled = false;
        }

        firstFrame  = AC.keys[0];
        lastFrame   = AC.keys[n_keys-1];
    }

    // Update is called once per frame
    void Update()
    {
        elapsed_time += rewind_read ? (-1)*Time.deltaTime : Time.deltaTime; // TODO : factorize in a scene global timer ?
        updateIntensity();
    }

    private void updateIntensity()
    {
        if (elapsed_time > lastFrame.time)
            rewind_read = true;
        if (elapsed_time < firstFrame.time)
            rewind_read = false;

        float val = AC.Evaluate(elapsed_time);

        target_light.intensity = base_intensity * val;
    }
}
