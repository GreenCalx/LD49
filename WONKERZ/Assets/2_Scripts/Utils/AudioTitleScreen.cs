using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Play only once on awake.
public class AudioTitleScreen : MonoBehaviour
{
    static bool played = false;
    void Awake() {
        if (!played)
        {
            played = true;
            GetComponent<AudioSource>().Play();
        }
    }
}
