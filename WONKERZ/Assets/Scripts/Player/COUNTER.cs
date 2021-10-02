using UnityEngine;

public class COUNTER : MonoBehaviour
{
    public Rigidbody Player;
    public TMPro.TextMeshProUGUI Text;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var PlayerVelocity = Player.velocity;
        var PlayerForward = Player.transform.forward;
        var Counter = new Vector3(PlayerVelocity.x * PlayerForward.x, PlayerVelocity.y * PlayerForward.y, PlayerVelocity.z * PlayerForward.z).magnitude;
        Text.SetText(((int)Counter).ToString());
    }
}
