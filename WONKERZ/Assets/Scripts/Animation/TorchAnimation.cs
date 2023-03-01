using UnityEngine;

public class TorchAnimation : MonoBehaviour
{
    public AnimationCurve AC;
    public Light target_light;
    private float elapsed_time;
    private float base_intensity;

    // Start is called before the first frame update
    void Start()
    {
        elapsed_time = 0f;
        base_intensity = target_light.intensity;
    }

    // Update is called once per frame
    void Update()
    {
        elapsed_time += Time.deltaTime; // TODO : factorize in a scene global timer ?
        updateIntensity();
    }

    private void updateIntensity()
    {
        float val = AC.Evaluate(elapsed_time);
        target_light.intensity = base_intensity * val;
    }
}
