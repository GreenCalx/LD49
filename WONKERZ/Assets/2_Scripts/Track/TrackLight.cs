using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackLight : MonoBehaviour
{
    public Light h_selfLight;
    public VolumetricSpotLight h_volumetricLight;

    public void ToggleLight(bool iState)
    {
        if (h_selfLight==null)
            return;
        h_selfLight.gameObject.SetActive(iState);
    }

    public void ToggleVolume(bool iState)
    {
        if (h_volumetricLight==null)
            return;
        h_volumetricLight.gameObject.SetActive(iState);
    }

}
