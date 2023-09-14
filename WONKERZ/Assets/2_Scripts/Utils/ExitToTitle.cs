using UnityEngine;
using UnityEngine.SceneManagement;
using Schnibble;

public class ExitToTitle : MonoBehaviour, IControllable
{
    public bool enabler;
    // Start is called before the first frame update
    void Start()
    {
        Utils.attachControllable<ExitToTitle>(this);
    }

    void OnDestroy()
    {
        Utils.detachControllable<ExitToTitle>(this);
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
