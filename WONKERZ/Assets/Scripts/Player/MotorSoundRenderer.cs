using UnityEngine;

public class MotorSoundRenderer : MonoBehaviour
{
    public Rigidbody Player;
    // Start is called before the first frame update
    void Start()
    {
        if (Player==null)
            Player = Utils.getPlayerRef().GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<AudioSource>().pitch = 0.8f + 1 / ((100) / (Player.velocity.magnitude));
    }
}
