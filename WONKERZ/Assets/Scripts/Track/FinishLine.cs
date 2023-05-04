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
        CarController cc = iCol.GetComponent<CarController>();
        if (!end_triggered && !!cc)
        {
            end_triggered = true;
            Access.TrackManager().endTrack();
            Access.SceneLoader().loadScene(Constants.SN_FINISH);
        }
    }
}
