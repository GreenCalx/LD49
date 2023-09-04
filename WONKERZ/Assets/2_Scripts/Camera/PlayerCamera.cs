using UnityEngine;

public class PlayerCamera : GameCamera
{
    [System.Serializable]
    public struct FOVEffect
    {
        [Range(0.01f, 1f)]
        public float thresholdPerCent;
        [Range(0.01f, 10.00f)]
        public float speed;
        [Range(50, 120)]
        public int max;
    }
    public FOVEffect fov;

    public GameObject playerRef;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void applySpeedEffect(float iSpeedPerCent)
    {
        Camera cam = GetComponent<Camera>();

        float apply_factor = Mathf.Clamp01((Mathf.Abs(iSpeedPerCent) - fov.thresholdPerCent) / fov.thresholdPerCent);

        float nextValue = initial_FOV + initial_FOV * apply_factor;
        float newFOV = Mathf.Clamp(nextValue, initial_FOV, fov.max);

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, newFOV, Time.deltaTime * fov.speed);
    }


    public override void init()
    {
        playerRef = Utils.getPlayerRef();
    }
}
