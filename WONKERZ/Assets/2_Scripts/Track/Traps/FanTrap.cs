using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanTrap : MonoBehaviour
{
    public bool isTriggered = false;
    public ParticleSystem WindPS_Ref;
    public GameObject WindCollider;
    public Animation FanAnimator;

    public float force = 10f;
    ///
    private Rigidbody rbToMove;

    // Start is called before the first frame update
    void Start()
    {
        Trigger(true);
        FanAnimator.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Trigger(bool iState)
    {
        if (iState)
        {
            WindPS_Ref.Play();
            WindCollider.SetActive(true);
            foreach (AnimationState state in FanAnimator)
            {
                state.speed = 1F;
            }
            
            
        } else {
            WindPS_Ref.Stop();
            WindCollider.SetActive(false);
            foreach (AnimationState state in FanAnimator)
            {
                state.speed = 0.2F;
            }
        }
        isTriggered = iState;
    }

    public void PushBackPlayer()
    {
        PlayerDetector pd = WindCollider.GetComponent<PlayerDetector>();
        Rigidbody rb = Access.Player().car.rb;
        
        Vector3 dir = transform.right;
        rb.AddForce( dir * force * Time.deltaTime, ForceMode.VelocityChange );

    }
}
