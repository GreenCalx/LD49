using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketBallBooth : MonoBehaviour
{
    public Canon canon;
    public bool eventIsActive = false;
    [Header("Tweaks")]
    public float canon_RoF = 2f;
    [Header("Internals")]
    public short score = 0;
    private float rest_time = 0f;

    // Start is called before the first frame update
    void Start()
    {
        rest_time = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (eventIsActive)
        {
            rest_time += Time.deltaTime;
            if (rest_time > canon_RoF)
            {
                canon.Fire();
                rest_time = 0f;
            }
        }
    }
}
