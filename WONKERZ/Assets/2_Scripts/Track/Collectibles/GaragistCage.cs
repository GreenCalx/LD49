using UnityEngine;
using UnityEngine.SceneManagement;

public class GaragistCage : MonoBehaviour
{
    public string openingKeyName;

    public Animator cageAnim;
    private string openCageAnimParm = "unlock";

    public GaragistCollectible garagist;
    public string trackname = "";

    void Start()
    {
        if (Access.CollectiblesManager().hasGaragist(trackname))
        {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider iCollider)
    {
        if (Utils.isPlayer(iCollider.gameObject))
        {
            string scene_name = SceneManager.GetActiveScene().name;
            if (Access.CollectiblesManager().hasCageKey(scene_name))
            {
                // open cage
                cageAnim.SetBool(openCageAnimParm, true);
                garagist.ForceCollect();
            }
        }
    }
}
