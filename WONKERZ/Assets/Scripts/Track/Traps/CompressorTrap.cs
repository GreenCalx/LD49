using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Face to face compressors 
public class CompressorTrap : MonoBehaviour
{
    public GameObject comp1;
    public GameObject comp2;
    private CompressorBlock cBlock1;
    private CompressorBlock cBlock2;
    
    public float speed = 10.0F;
    public float closureOffset = 1f;
    
    private float startTime;
    private float journeyLength1;
    private float journeyLength2;
    private bool isClosing;

    private Vector3 middle;
    private Vector3 origComp1;
    private Vector3 origComp2;
    private Vector3 virtualPos1;
    private Vector3 virtualPos2;
    private float lastDistanceToBall1;
    private float lastDistanceToBall2;

    // Start is called before the first frame update
    void Start()
    {
        cBlock1 = comp1.GetComponent<CompressorBlock>();
        cBlock2 = comp2.GetComponent<CompressorBlock>();

        origComp1   = comp1.transform.position;
        origComp2   = comp2.transform.position;
        virtualPos1 = origComp1;
        virtualPos2 = origComp2;
        middle      = Vector3.Lerp(origComp1, origComp2, 0.5f);

        journeyLength1 = Vector3.Distance(origComp1, middle);
        journeyLength2 = Vector3.Distance(origComp2, middle);

        startTime = Time.time;
        isClosing = true;
    }

    // Update is called once per frame
    void Update()
    {
        float distCovered = (Time.time - startTime) * speed;
        float fractionOfJourney1 = distCovered / journeyLength1;
        float fractionOfJourney2 = distCovered / journeyLength2;

        if (isClosing)
        {
            virtualPos1 = Vector3.Lerp(origComp1, middle, fractionOfJourney1);
            virtualPos2 = Vector3.Lerp(origComp2, middle, fractionOfJourney2);
        } else {
            virtualPos1 = Vector3.Lerp(middle, origComp1, fractionOfJourney1);
            virtualPos2 = Vector3.Lerp(middle, origComp2, fractionOfJourney2);   
        }

        float distToMid1 = Vector3.Distance( virtualPos1, middle);
        float distToMid2 = Vector3.Distance( virtualPos2, middle);

        if ((distToMid1==journeyLength1) && (distToMid2==journeyLength2))
        {
            isClosing = true; startTime = Time.time;
        } else if ((distToMid1==0f) && (distToMid2==0f)) {
            isClosing = false; startTime = Time.time;
        }
        
        if (!cBlock1.playerInBall)
        {
            comp1.transform.position = virtualPos1;
        } else {
            lastDistanceToBall1 = Vector3.Distance(comp1.transform.position, cBlock1.ballPowerPos);
        }

        if (!cBlock2.playerInBall)
        {
            comp2.transform.position = virtualPos2;
        } else {
            lastDistanceToBall2 = Vector3.Distance(comp2.transform.position, cBlock2.ballPowerPos);
        }
        
        if ((!!cBlock2.playerInBall || !!cBlock1.playerInBall) && !isClosing)
        {
            float virtualToCurrentPos1 = Vector3.Distance(comp1.transform.position, virtualPos1);
            float virtualToCurrentPos2 = Vector3.Distance(comp2.transform.position, virtualPos2);
            if ( virtualToCurrentPos1 >= lastDistanceToBall1)
            { comp1.transform.position = virtualPos1; }
            if ( virtualToCurrentPos2 >= lastDistanceToBall2)
            { comp2.transform.position = virtualPos2; }
        }

    }
}
