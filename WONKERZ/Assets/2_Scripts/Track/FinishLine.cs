using UnityEngine;
using UnityEngine.SceneManagement;
using Schnibble;

namespace Wonkerz {
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
            if (!end_triggered && Utils.colliderIsPlayer(iCol))
            {
                end_triggered = true;

                Access.managers.trackMgr.endTrack();
            
                TrickTracker tt = Access.Player().gameObject.GetComponent<TrickTracker>();
                if (!!tt)
                {
                    Access.managers.trackMgr.addToScore(tt.storedScore);
                    tt.storedScore = 0;
                }
            

                Access.managers.sceneMgr.loadScene(Constants.SN_FINISH, new SceneLoader.LoadParams{
                    useTransitionIn = true,
                    useTransitionOut = true,
                });
            }
        }
    }
}
