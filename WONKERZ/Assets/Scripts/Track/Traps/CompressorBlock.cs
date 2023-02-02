using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompressorBlock : MonoBehaviour
{
    public bool playerInBall = false;
    public Vector3 ballPowerPos;

    void Start()
    {
        playerInBall        = false;
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
        CarController cc    = iCollision.collider.gameObject.GetComponent<CarController>();
        BallPowerObject bpo = iCollision.collider.gameObject.GetComponent<BallPowerObject>();
        if (!!bpo)
            ballPowerPos = bpo.transform.position;
        else
            ballPowerPos = Access.Player().transform.position;

        playerInBall = !!bpo;
        if ( !playerInBall && !!cc && !(Access.Player().CurrentMode==CarController.CarMode.BALL))
        {
            Access.Player().kill();
        }
    }
}
