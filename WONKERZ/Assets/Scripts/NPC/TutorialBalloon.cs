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
    public List<Transform> path;
    public bool enable_move = true;
    
    [Header("MAND")]
    public UIGenerativeTextBox display;
    private PlayerController player;
    public Transform movingInfoPanel;
    public RectTransform attachedPanelUI;

    private int path_index = 0;
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
        path_index = 0;
        elapsedTimeSinceStart = 0f;

        if ((path == null)||(path.Count==0))
        {enable_move = false; Debug.LogError("Path missing in TutorialBalloon.");}

        facePlayer();
    }
    void Update()
    {
        if (enable_move)
        {
            float distanceCheck = distFromPlayer;  
            if (Vector3.Distance(player.transform.position, transform.position) <= distFromPlayer)
            {
                move();
            } else {
                
            }
            wiggle();
            facePlayer();
        }

    }
    public void move()
    {
        transform.position = Vector3.Lerp(transform.position, path[path_index].position, Time.deltaTime*speed );

        if (Vector3.Distance(transform.position, path[path_index].position) < 5f)
        {
            path_index++;
            if (path_index >= path.Count)
            {
                enable_move = false;
                return;
            }
        }
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
        Vector3 difference = player.transform.position - movingInfoPanel.position;
        float rotationY = Mathf.Atan2(difference.x, difference.z) * Mathf.Rad2Deg;

        movingInfoPanel.rotation = Quaternion.Euler(0.0f, rotationY, 0.0f);
        attachedPanelUI.rotation = Quaternion.Euler(0.0f, rotationY + 180 , 0.0f);
    }

}
