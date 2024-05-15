using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingMortar : Bomb
{
    [Header("Refs")]
    public Transform self_BallMesh;
    public LineRenderer self_AtkWarnLine;
    public Transform self_DistLabelCanvas;
    public TMPro.TextMeshProUGUI self_DistLabel;
    [Header("Tweaks")]
    public float initHeightFromTarget = 1000f;
    public float startDownImpulse = 1f;
    public float timeBeforeFallingOnTarget = 2f;
    [Header("Internals")]
    public bool isFalling = false;
    public Vector3 positionTarget;
    private float elapsedTime;
    private Rigidbody rb;
    private SphereCollider sc;
    // Start is called before the first frame update
    void Start()
    {
        elapsedTime = 0f;
        isFalling = false;
        rb = self_BallMesh.GetComponent<Rigidbody>();
        sc = self_BallMesh.GetComponent<SphereCollider>();
        sc.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isFalling)
        {
            if (elapsedTime < timeBeforeFallingOnTarget)
            {
                elapsedTime += Time.deltaTime;
                //self_BallMesh.position = new Vector3(positionTarget.x, positionTarget.y + initHeightFromTarget, positionTarget.z);
                return;
            }

            // let it fall !

            rb.velocity = Vector3.zero;
            sc.enabled = true;

            self_BallMesh.position = new Vector3(positionTarget.x, positionTarget.y + initHeightFromTarget, positionTarget.z);
            rb.AddForce(Vector3.up * startDownImpulse * -1, ForceMode.Impulse);

            isFalling = true;
        }

        UpdateAtkWarnLine();
        
    }

    public void UpdateAtkWarnLine()
    {
        if (self_AtkWarnLine==null)
            return;

        self_AtkWarnLine.SetPosition(0, self_BallMesh.position);
        self_AtkWarnLine.SetPosition(1, positionTarget);

        if (self_DistLabel==null)
            return;

        float dist = Vector3.Distance(self_BallMesh.position, positionTarget);
        float vel = rb.velocity.magnitude;

        float time = MathF.Round( (dist / vel), 1);

        self_DistLabel.text = time.ToString() + "s";
        self_DistLabelCanvas.transform.position = positionTarget;


    }

}
