using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Schnibble;

public class StartLine : MonoBehaviour
{
    [Header("MAND")]
    //public string track_name;
    public GameObject UIStartTrackRef;
    public AudioSource  startLineCrossed_SFX;
    public bool destroyOnActivation = true;
    public float uiShowTime = 2f;

    public bool enable_tricks = false;

    private GameObject UIStartTrackInst = null;


    private float uiVisibleElapsed = 0f;
    // Start is called before the first frame update
    void Start()
    {
        uiVisibleElapsed = 0f;
    }

    void Update()
    {
        if (UIStartTrackInst != null)
        {
            uiVisibleElapsed += Time.deltaTime;
            if (uiVisibleElapsed >= uiShowTime)
            {
                Destroy(UIStartTrackInst.gameObject);
                if (destroyOnActivation)
                {
                    Destroy(gameObject);
                }
                
            }
        }
    }

    void OnTriggerEnter(Collider iCol)
    {
        if (Utils.colliderIsPlayer(iCol))
        {
            if (UIStartTrackInst==null)
            {
                UIStartTrackInst = Instantiate(UIStartTrackRef);
                uiVisibleElapsed = 0f;
            }

            if (!!startLineCrossed_SFX)
            {
                startLineCrossed_SFX.Play();
            }
            
            // Reset track infinite collectibles
            Access.CollectiblesManager().resetInfCollectibles();

            // start line crossed !! gogogo
            Scene currentScene = SceneManager.GetActiveScene();
            Access.TrackManager().launchTrack(currentScene.name);

            if (enable_tricks)
                activateTricks();
        }
    }

    private void activateTricks()
    {
        TrickTracker tt = Access.Player().gameObject.GetComponent<TrickTracker>();
        if (!!tt)
        {
            tt.activate_tricks = true; // activate default in hub
            tt.init();
        }
    }
}
