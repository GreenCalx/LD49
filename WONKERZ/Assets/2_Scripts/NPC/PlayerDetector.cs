using UnityEngine;
using UnityEngine.Events;
using Schnibble;

using Schnibble.AI;

public class PlayerDetector : SchAIDetector
{
    public bool playerInRange = false;
    public Transform player;

    public Transform dummy;
    public bool dummyInRange = false;

    [Header("Optionals")]
    public UnityEvent callBackOnPlayerEnterRange;
    public UnityEvent callBackOnPlayerInRange;
    public UnityEvent callbackOnPlayerExitRange;
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

    public override bool IsTargetInRange() { return playerInRange; }
    public override GameObject GetTarget() { return (!!player)?player.gameObject:null; }

    void OnTriggerStay(Collider iCollider)
    {
        // If the player dies while in range
        if (Utils.colliderIsPlayer(iCollider))
        {
            if (callBackOnPlayerInRange!=null)
                callBackOnPlayerInRange.Invoke();
        }
    }

    // if player is out of range
    void OnTriggerExit(Collider iCollider)
    {
        if (!playerInRange)
            return;

        if (Utils.colliderIsPlayer(iCollider))
        {
            playerInRange = false;
            player = null;
            if (callbackOnPlayerExitRange!=null)
            callbackOnPlayerExitRange.Invoke();
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
        if (playerInRange)
            return;

        if (Utils.colliderIsPlayer(iCollider))
        {
            playerInRange = true;

             // If a child collider of Player is detected
             // We still want to keep ref on the player object
             // thus we set it to raw player instead of incoming collider
            player = Access.Player().transform;

            if (callBackOnPlayerEnterRange!=null)
            callBackOnPlayerEnterRange.Invoke();
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
