using UnityEngine;

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
            dummy = null;
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
            dummy = d.transform;
        }
    }
}
