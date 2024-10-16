using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackLight : MonoBehaviour
{
    public Light h_selfLight;

    public void ToggleLight(bool iState)
    {
        h_selfLight.gameObject.SetActive(iState);
    }

}
