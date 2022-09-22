using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleNut : AbstractCollectible
{
    public float animTimeStep = 0.1f;
    public bool startUp;

    [Range(0f,10f)]
    public float yOscillation;
    [Range(0f,5f)]
    public float oscillationSpeed;
    
    public float yRotationSpeed;
    ///
    private Vector3 initPos;
    private Vector3 minPos;
    private Vector3 maxPos;
    private Vector3 initRot;
    private float elapsedTime;
    private bool isGoingUp;
    private float travelTime;
    private float startTime;

    // Start is called before the first frame update
    void Start()
    {
        initRot = transform.eulerAngles;
        minPos = transform.position - new Vector3(0f, yOscillation, 0f);
        maxPos = transform.position + new Vector3(0f, yOscillation, 0f);
        elapsedTime = 0f;
        isGoingUp = !startUp;
        travelTime = Vector3.Distance(minPos, maxPos);
        startTime = Time.time;
        ///
        transform.position = (startUp) ? maxPos : minPos;
    }

    // Update is called once per frame
    void Update()
    {
        //elapsedTime += Time.deltaTime;
        //if (elapsedTime >= animTimeStep)
            animate();
    }

    void animate()
    {
        transform.Rotate( new Vector3(0,yRotationSpeed,0), Space.World);
        ///
        float distCovered = (Time.time - startTime) * oscillationSpeed;
        Vector3 nextPos = (isGoingUp)? Vector3.Lerp( minPos, maxPos, distCovered / travelTime ) :
                                       Vector3.Lerp( maxPos, minPos, distCovered / travelTime) ;
        if ( isGoingUp && (nextPos.y > (maxPos.y - (distCovered / travelTime)/100)) )
        { isGoingUp =! isGoingUp; startTime = Time.time; }
        else if ( !isGoingUp && (nextPos.y < (minPos.y + (distCovered / travelTime)/100)) )
        { isGoingUp =! isGoingUp; startTime = Time.time; }

        transform.position = nextPos;
        elapsedTime = 0f;
    }

    protected override void OnCollect()
    {
        gameObject.SetActive(false);
        //TODO : persist collected status
    }
}
