using UnityEngine;
using Schnibble;

public class DeathZone : MonoBehaviour
{
    public CheckPointManager checkPointManager;
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
        if (Utils.colliderIsPlayer(iCol))
        {
            checkPointManager.loadLastCP();
        }
    }

    void OnCollisionEnter(Collision iCol)
    {
        this.Log(iCol.gameObject.name);
        if (Utils.collisionIsPlayer(iCol))
        {
            checkPointManager.loadLastCP();
        }
    }
}
