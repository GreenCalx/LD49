using UnityEngine;

public class MotorSoundRenderer : MonoBehaviour
{
    public Rigidbody PLayer;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<AudioSource>().pitch = 0.8f + 1 / ((100) / (PLayer.velocity.magnitude));
    }
}
