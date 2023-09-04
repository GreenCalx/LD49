using UnityEngine;
using Schnibble;

public class DeathZone : MonoBehaviour
{
    private CheckPointManager checkPointManager;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider iCol)
    {
        if (checkPointManager==null)
        {
            checkPointManager = Access.CheckPointManager();
        }

        if (Utils.colliderIsPlayer(iCol))
        {
            checkPointManager.loadLastCP();
        }
    }

    void OnCollisionEnter(Collision iCol)
    {
        if (checkPointManager==null)
        {
            checkPointManager = Access.CheckPointManager();
        }

        if (Utils.collisionIsPlayer(iCol))
        {
            checkPointManager.loadLastCP();
        }
    }
}
