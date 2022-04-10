using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICurveSelector : MonoBehaviour
{
    [Header("Tweaks")]
    public float moveStep = 1f;
    
    [Header("Internals")]
    public float XLeftBound;
    public float XRightBound;
    public float XKeyLeftBound;
    public float XKeyRightBound;
    public GarageUICarStats observer;
    public int movable_key;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float X = Input.GetAxis("Horizontal");
        if( X > 0.2f)
        {
            if ((transform.position.x < XRightBound) && (transform.position.x < XKeyRightBound) )
            {
                transform.position += new Vector3( moveStep, 0f, 0f);
                observer.notifySliderMove(movable_key);
            }
        } else if ( X < -0.2f) 
        {
            if ((transform.position.x > XLeftBound) && (transform.position.x > XKeyLeftBound))
            {
                observer.notifySliderMove(movable_key);
                transform.position -= new Vector3( moveStep, 0f, 0f);
            }
        }

    }
}
