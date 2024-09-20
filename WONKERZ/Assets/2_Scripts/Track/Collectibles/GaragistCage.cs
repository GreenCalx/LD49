using UnityEngine;
using UnityEngine.SceneManagement;
namespace Wonkerz
{

    public class GaragistCage : MonoBehaviour
    {
        public string openingKeyName;

        public Animator cageAnim;
        private string openCageAnimParm = "unlock";

        public GaragistCollectible garagist;
        public NPCGaragist NPC;
        public string trackname = "";

        void Start()
        {
            if (Access.managers.collectiblesMgr.hasGaragist(trackname))
            {
                gameObject.SetActive(false);
            }
            else
            {
                NPC.setJailed();
            }
        }


        public void TryOpenCage()
        {
            if (NPC.isFree)
            return;

            string scene_name = SceneManager.GetActiveScene().name;
            if (Access.managers.collectiblesMgr.hasCageKey(scene_name))
            {
                // open cage
                cageAnim.SetBool(openCageAnimParm, true);

                NPC.setFree();
                garagist.ForceCollect();
            }
            else
            {
                NPC.StartTalk();
            }
        }

    }
}
