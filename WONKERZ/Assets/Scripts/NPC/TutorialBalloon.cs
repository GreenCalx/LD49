using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;

public class TutorialBalloon : MonoBehaviour
{
    [Header("Init")]
    public float speed = 1f;
    public float distFromPlayer = 50f;
    public float extraDistanceToMoveAgainWhenStopped = 5f;
    public bool enable_move = true;
    
    [Header("MAND")]
    public UIGenerativeTextBox display;
    private PlayerController player;
    public SplineWalker splineWalker;

    private float elapsedTimeSinceStart = 0f;
    public void updateDisplay(List<UIGenerativeTextBox.UIGTBElement> iElems)
    {
        display.elements = iElems;
        display.resetAndGenerate();
    }

    // Start is called before the first frame update
    void Start()
    {
        player = Access.Player();
        elapsedTimeSinceStart = 0f;

        facePlayer();
    }

    void Update()
    {
        if (enable_move)
        {
            float distanceCheck = distFromPlayer;  
            if (Vector3.Distance(player.transform.position, transform.position) <= distFromPlayer)
            {
                splineWalker.enabled = true;
            } else {
                splineWalker.enabled = false;
            }
            //wiggle();
            facePlayer();
        }
    }

    public void move()
    {

    }

    public void stop()
    {
        
    }

    public void wiggle()
    {
        Vector3 newPosition = transform.position;

        elapsedTimeSinceStart += Time.deltaTime;
        newPosition.y += Mathf.Sin(elapsedTimeSinceStart) * 0.005f;

        transform.position = newPosition;
    }

    private void facePlayer()
    {
        Vector3 difference = player.transform.position - transform.position;
        float rotationY = Mathf.Atan2(difference.x, difference.z) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0.0f, rotationY, 0.0f);
    }

}
