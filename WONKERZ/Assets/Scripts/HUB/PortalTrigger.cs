using UnityEngine;
using Schnibble;

public class PortalTrigger : MonoBehaviour
{
    HUBPortal portal;
    public bool isActive = false;

    // Start is called before the first frame update
    void Start()
    {
        portal = GetComponentInParent<HUBPortal>(true);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider iCollider)
    {
        if (!isActive)
            return;

        PlayerController player = iCollider.GetComponent<PlayerController>();
        if (!!player && !!portal)
        {
            portal.activatePortal();
        }
    }
}
