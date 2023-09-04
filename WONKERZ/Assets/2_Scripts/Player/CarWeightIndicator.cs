using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarWeightIndicator : MonoBehaviour
{
    public GameObject carCOM;
    public GameObject indicator;
    public float indicatorHeight;
    // Update is called once per frame
    void Update()
    {
        indicator.transform.position = carCOM.transform.TransformPoint(Vector3.Scale(carCOM.transform.localPosition, new Vector3(1,0,1)) + indicatorHeight * Vector3.up) ;
    }
}
