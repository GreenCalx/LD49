using UnityEngine;
using UnityEngine.SceneManagement;
using Schnibble;
using Schnibble.Managers;

namespace Wonkerz {
    public class ExitToTitle : MonoBehaviour, IControllable
    {
        public bool enabler;
        // Start is called before the first frame update
        void Start()
        {
            Access.managers.playerInputsMgr.player1.Attach(this as IControllable);
        }

        void OnDestroy()
        {
            Access.managers.playerInputsMgr.player1.Detach(this as IControllable);
        }

        void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
        {
            if (enabler)
            {
                if ((Entry.Get(PlayerInputs.GetIdx("Cancel")) as GameInputButton).GetState().down)
                SceneManager.LoadScene(Constants.SN_TITLE, LoadSceneMode.Single);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
