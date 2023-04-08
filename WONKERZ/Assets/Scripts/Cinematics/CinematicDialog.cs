using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PNJDialog))]
public class CinematicDialog : MonoBehaviour
{
    public float auto_talk_time = 5f;

    private float internalTimer = 0f;
    private PNJDialog dialog;

    public void playPNJDialog()
    {
        StartCoroutine(autoTalk());
    }

    // -------------------------------------
    // COROUTINES
    IEnumerator autoTalk()
    {
        while(dialog.talk())
        {
            while (internalTimer < auto_talk_time)
            {
                internalTimer += Time.deltaTime;
                yield return null;
            }
        }        
    }

    // -------------------------------------
    // UNITY
    void Start()
    {
        dialog = GetComponent<PNJDialog>();
    }
}
