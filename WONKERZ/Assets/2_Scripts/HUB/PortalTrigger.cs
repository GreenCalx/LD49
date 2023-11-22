using UnityEngine;
using Schnibble;

public class PortalTrigger : MonoBehaviour
{
    public HUBPortal portal;
    public bool isActive = false;
    public bool authorizeCinematicPlayerTrigger = true;

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
        tryActivate(iCollider);
    }

    void OnTriggerStay(Collider iCollider)
    {
        tryActivate(iCollider);
    }

    private void tryActivate(Collider iCollider)
    {
        if (!isActive)
            return;

        if (!!Utils.colliderIsPlayer(iCollider) && !!portal)
        {
            portal.activatePortal();
            isActive = false;
            return;
        }
        if (authorizeCinematicPlayerTrigger)
        {
            CinematicPlayerTrigger cinePlayer = iCollider.gameObject.GetComponent<CinematicPlayerTrigger>();
            if (!!cinePlayer)
            {
                portal.activatePortal();
                isActive = false;
                return;
            }
        }
    }
}
