using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PushButton : MonoBehaviour
{

    public Transform self_topBtnRef;
    public Transform self_topBtnLowLimit;
    public Transform self_topBtnUpLimit;
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
        Physics.IgnoreCollision(GetComponent<Collider>(), self_topBtnRef.GetComponent<Collider>());
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
    void FixedUpdate()
    {
        self_topBtnRef.transform.localPosition = new Vector3(0, self_topBtnRef.localPosition.y, 0);
        self_topBtnRef.transform.localEulerAngles = new Vector3(0,0,0);
        if (self_topBtnRef.position.y >= self_topBtnUpLimit.localPosition.y)
        {
            self_topBtnRef.transform.position = new Vector3(self_topBtnRef.position.x, self_topBtnRef.position.y, self_topBtnRef.position.z);
        } else {
            self_topBtnRef.GetComponent<Rigidbody>().AddForce(self_topBtnRef.transform.up * force * Time.fixedDeltaTime);
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
