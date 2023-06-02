using UnityEngine;

public class COUNTER : MonoBehaviour
{
    public TMPro.TextMeshProUGUI Text;
    void Update()
    {
        var PlayerVelocity = Access.Player().rb.velocity.magnitude;
        Text.SetText(((int)PlayerVelocity).ToString());
    }
}
