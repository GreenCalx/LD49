#if false


using UnityEngine;
using Schnibble;

public class CompressorBlock : MonoBehaviour
{
    public bool playerInBall = false;
    public Vector3 ballPowerPos;

    void Start()
    {
        playerInBall = false;
    }

    void OnCollisionEnter(Collision iCollision)
    {
        collisionEffect(iCollision);
    }
    void OnCollisionStay(Collision iCollision)
    {
        //collisionEffect(iCollision);
    }
    void OnCollisionExit(Collision iCollision)
    {
        playerInBall = false;
    }

    void collisionEffect(Collision iCollision)
    {
        CarController cc = iCollision.collider.gameObject.GetComponent<CarController>();
        BallPowerObject bpo = iCollision.collider.gameObject.GetComponent<BallPowerObject>();
        if (!!bpo)
            ballPowerPos = bpo.transform.position;
        else
            ballPowerPos = Access.Player().transform.position;

        playerInBall = !!bpo;
        if (!playerInBall && !!cc && !(Access.Player().fsm.currentState == Access.Player().fsm.states[(int)PlayerFSM.States.Ball]))
        {
            Access.Player().Kill();
        }
    }
}

#endif
