using UnityEngine;
using Schnibble;

namespace Wonkerz {

public class CheckPoint : AbstractCameraPoint
{
    [Header("v CheckPoint v")]
    [Header("MAND")]
    public Transform respawn_location;
    public AudioSource  checkpoint_SFX;
    public int id;

    [Header("Tweaks")]
    public string checkpoint_name;
    public bool activateCPFromColliderTrigger = false;
    private bool triggered = false;

    private CheckPointManager CPM;
    private CollectiblesManager CM;

    private TrickTracker TT;

    // Start is called before the first frame update
    void Start()
    {
        if (checkpoint_name=="")
        checkpoint_name = gameObject.name;

        CPM = Access.CheckPointManager();
    }

    public bool subscribeToManager(CheckPointManager iCPM)
    {
        CPM = iCPM;
        return CPM.subscribe(this.gameObject);
    }

    void OnTriggerEnter(Collider iCol)
    {
        if (!activateCPFromColliderTrigger)
        return;

        if (triggered)
        return;

        if (!Utils.colliderIsPlayer(iCol))
        return;

        triggerCheckPoint();

        if (!!checkpoint_SFX)
        checkpoint_SFX.Play();
    }

    public void triggerCheckPoint()
    {
        CPM.notifyCP(gameObject, true);
        triggered = true;
    }

    public GameObject getSpawn()
    {
        return respawn_location.gameObject;
    }

}
}
