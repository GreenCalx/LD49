using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public Transform IsUpAnchor;
    public Transform IsDownAnchor;

    Rigidbody rb;

    public LayerMask presserLayer;
    public float forceStrength = 1f;
    public float anchorCheckThreshold = 0.1f;

    public bool isTriggered;

    // Start is called before the first frame update
    void Start()
    {
        isTriggered = false;
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, IsUpAnchor.position) <= anchorCheckThreshold)
        {
            transform.position = IsUpAnchor.position;
            rb.isKinematic = true;
        }
        else if (Vector3.Distance(transform.position, IsDownAnchor.position) <= anchorCheckThreshold)
        {
            transform.position = IsDownAnchor.position;
            isTriggered = true;
        }

        rb.AddForce(Vector3.up * forceStrength, ForceMode.Force);

    }

    void OnCollisionEnter(Collision iCol)
    {
        if ((presserLayer.value & (1 << iCol.transform.gameObject.layer)) > 0)
        {
            rb.isKinematic = false;
        }
    }

}
