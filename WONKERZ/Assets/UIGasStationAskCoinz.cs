using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGasStationAskCoinz : MonoBehaviour
{
    public Texture2D texture;

    public float scaleAnimSpeed;
    public float scaleAnimSize;

    public bool animate = false;
    private Vector3 initialScale;
    void Start()
    {
        initialScale = transform.localScale;
    }
    // Update is called once per frame
    void Update()
    {
        if (animate)
        {
            var value = Mathf.Sin(Time.realtimeSinceStartup * scaleAnimSpeed);
            transform.localScale = initialScale + new Vector3(value, value, value) * scaleAnimSize;
        }
        else
        {
            transform.localScale = initialScale;
        }
    }
}
