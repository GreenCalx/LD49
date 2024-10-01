using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class UIColorMover : MonoBehaviour
{
    public Image image;
    public float switchRate;
    float switchTimer;

    Color colorPrevious;
    Color colorNext;

    void Update() {
        if (switchTimer > 0.0f) switchTimer -= Time.unscaledDeltaTime;
        else {
            colorPrevious = colorNext;
            colorNext     = Random.ColorHSV();
            switchTimer   = switchRate;
        }

        image.color = Color.Lerp(colorNext, colorPrevious, switchTimer / switchRate);
    }
}
