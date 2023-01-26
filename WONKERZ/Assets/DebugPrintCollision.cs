using UnityEngine;

public class DebugPrintCollision : MonoBehaviour
{
    void OnCollisionEnter(Collision c)
    {
        Debug.Log("CollisionEnter between " + gameObject.name + " and " + c.collider.gameObject.name);
    }

    void OnCollisionExit(Collision c)
    {

        Debug.Log("CollisionExit between " + gameObject.name + " and " + c.collider.gameObject.name);
    }

    void OnCollisionStay(Collision c)
    {

        Debug.Log("CollisionStay between " + gameObject.name + " and " + c.collider.gameObject.name);
    }
}
