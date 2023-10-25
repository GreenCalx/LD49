using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PushButton : MonoBehaviour
{
    public Rigidbody buttonTopRB;
    public Transform self_topBtnRef;
    public Transform self_topBtnLowLimit;
    public Transform self_topBtnUpLimit;
    public Collider[] CollidersToIgnore;
    public float threshold;
    public float force = 10f;
    private float upperLowerDiff;
    public bool isPressed;
    private bool prevPressedState;

    public List<UnityEvent> OnPressed;
    public List<UnityEvent> OnReleased;

    // Start is called before the first frame update
    void Start()
    { 
        Collider localCollider = GetComponent<Collider>();
        
        if(!!localCollider)
        {
            Physics.IgnoreCollision( localCollider, self_topBtnRef.GetComponent<Collider>());
            foreach (Collider singleCollider in CollidersToIgnore)
            {
                Physics.IgnoreCollision(localCollider, singleCollider);
            }
        }


        if (transform.eulerAngles != Vector3.zero)
        {
            Vector3 savedeAngle = transform.eulerAngles;
            transform.eulerAngles = Vector3.zero;
            upperLowerDiff = self_topBtnUpLimit.position.y - self_topBtnLowLimit.position.y;
            transform.eulerAngles = savedeAngle;
        } else
        { 
            upperLowerDiff = self_topBtnUpLimit.position.y - self_topBtnLowLimit.position.y;
        }

    }

    // Update is called once per frame
    void Update()
    {
        self_topBtnRef.transform.localPosition = new Vector3(0, self_topBtnRef.localPosition.y, 0);
        self_topBtnRef.transform.localEulerAngles = new Vector3(0,0,0);
        if (self_topBtnRef.localPosition.y >= 0) 
        {
            self_topBtnRef.transform.position = new Vector3(self_topBtnUpLimit.position.x, self_topBtnUpLimit.position.y, self_topBtnUpLimit.position.z);
        } else {
            buttonTopRB.AddForce(self_topBtnRef.transform.up * force * Time.deltaTime);
        }

        if (self_topBtnRef.localPosition.y <= self_topBtnLowLimit.localPosition.y)
            self_topBtnRef.transform.position = new Vector3(self_topBtnLowLimit.position.x, self_topBtnLowLimit.position.y, self_topBtnLowLimit.position.z);

        isPressed = (Vector3.Distance(self_topBtnRef.position, self_topBtnLowLimit.position) < upperLowerDiff * threshold);

        if (isPressed && prevPressedState != isPressed)
            Pressed();
        if (!isPressed && prevPressedState != isPressed)
            Released();
    }

    void Pressed()
    {
        prevPressedState = isPressed;
        foreach (UnityEvent ev in OnPressed)
        {
            ev.Invoke();
        }
    }

    void Released()
    {
        prevPressedState = isPressed;
        foreach (UnityEvent ev in OnReleased)
        {
            ev.Invoke();
        }
    }
}
