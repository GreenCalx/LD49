using UnityEngine;
using UnityEngine.SceneManagement;
using Schnibble;

public class FinishLine : MonoBehaviour
{
    public bool end_triggered = false;

    // Start is called before the first frame update
    void Start()
    {
        end_triggered = false;

    }

    void OnTriggerEnter(Collider iCol)
    {
        PlayerController cc = iCol.GetComponent<PlayerController>();
        if (!end_triggered && !!cc)
        {
            end_triggered = true;

            Access.TrackManager().endTrack();
            
            TrickTracker tt = cc.GetComponent<TrickTracker>();
            if (!!tt)
            {
                Access.TrackManager().addToScore(tt.storedScore);
                tt.storedScore = 0;
            }
            

            Access.SceneLoader().loadScene(Constants.SN_FINISH);
        }
    }
}
