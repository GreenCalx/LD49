using UnityEngine;
using UnityEngine.SceneManagement;
using Schnibble;

public class ExitToTitle : MonoBehaviour, IControllable
{
    public bool enabler;
    // Start is called before the first frame update
    void Start()
    {
        Access.PlayerInputsManager().player1.Attach(this as IControllable);
    }

    void OnDestroy()
    {
        Access.PlayerInputsManager().player1.Detach(this as IControllable);
    }

    void IControllable.ProcessInputs(InputManager currentMgr, GameInput[] Entry)
    {
        if (enabler)
        {
            if ((Entry[PlayerInputs.GetIdx("Cancel")] as GameInputButton).GetState().down)
            SceneManager.LoadScene(Constants.SN_TITLE, LoadSceneMode.Single);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
