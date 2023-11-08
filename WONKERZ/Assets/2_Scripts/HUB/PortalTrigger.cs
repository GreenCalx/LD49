using UnityEngine;
using Schnibble;

public class PortalTrigger : MonoBehaviour
{
    public HUBPortal portal;
    public bool isActive = false;

    // Start is called before the first frame update
    void Start()
    {
        if (portal==null)
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

        if (!!Utils.colliderIsPlayer(iCollider) && !!portal)
        {
            portal.activatePortal();
            isActive = false;
        }
    }

    void OnTriggerStay(Collider iCollider)
    {
        if (!isActive)
            return;

        if (!!Utils.colliderIsPlayer(iCollider) && !!portal)
        {
            portal.activatePortal();
            isActive = false;
        }
    }
}
