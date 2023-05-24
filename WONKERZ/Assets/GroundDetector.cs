using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    public float detectionRelease = 1f;
    public bool crossedGround = false;

    private float elapsed = 0f;

    // Start is called before the first frame update
    void Start()
    {
        crossedGround = false;
        elapsed = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (crossedGround)
        {
            elapsed += Time.deltaTime;
            if (elapsed >= detectionRelease)
            {
                crossedGround = false;
            }
        }
    }

    void OnTriggerEnter(Collider iCol)
    {
        if (iCol.gameObject.GetComponent<Ground>())
        {
            crossedGround = true;
            elapsed = 0f;
        }
    }

    void OnTriggerStay(Collider iCol)
    {
        if (iCol.gameObject.GetComponent<Ground>())
        {
            crossedGround = true;
            elapsed = 0f;
        }
    }

    void OnTriggerExit(Collider iCol)
    {
        if (iCol.gameObject.GetComponent<Ground>())
        {
            crossedGround = true;
            elapsed = 0f;
        }
    }

}
