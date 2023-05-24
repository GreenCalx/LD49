using UnityEngine;
using Schnibble;

public class PlayerDetector : MonoBehaviour
{
    public bool playerInRange = false;
    public Transform player;

    public Transform dummy;
    public bool dummyInRange = false;
    // Start is called before the first frame update
    void Start()
    {
        playerInRange = false;
        dummyInRange = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerStay(Collider iCollider)
    {
        // If the player dies while in range
        if ((player!=null) && (player.GetComponent<CarController>()==null))
        {
            playerInRange = false;
            player = null;
        }
    }

    // if player is out of range
    void OnTriggerExit(Collider iCollider)
    {
        CarController cc = iCollider.GetComponent<CarController>();
        if (!!cc)
        {
            playerInRange = false;
            player = null;
        }

        Dummy d = iCollider.GetComponent<Dummy>();
        if (!!d)
        {
            dummyInRange = false;
            playerInRange = false;
            dummy = null;
            player = null;
        }
    }

    //player in range, stop rolling
    void OnTriggerEnter(Collider iCollider)
    {
        CarController cc = iCollider.GetComponent<CarController>();
        if (!!cc)
        {
            playerInRange = true;
            player = cc.transform;
        }

        Dummy d = iCollider.GetComponent<Dummy>();
        if (!!d)
        {
            dummyInRange = true;
            playerInRange = true;
            dummy = d.transform;
            player = d.transform;
        }
    }
}
