using UnityEngine;
using Schnibble;
using Schnibble.UI;
using Schnibble.Managers;

public class grapintest : MonoBehaviour, IControllable
{
    public PlayerController Player;
    public GameObject grapin;
    public Vector3 D;
    // Start is called before the first frame update
    void Start()
    {
        Access.Player().inputMgr.Attach(this as IControllable);
    }

    private void OnDestroy()
    {
        Access.Player().inputMgr.Detach(this as IControllable);
    }

    void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
    {
        Player.IsHooked = (Entry.Get((int) PlayerInputs.InputCode.Grapin) as GameInputButton).GetState().heldDown;
        grapin.SetActive((Entry.Get((int) PlayerInputs.InputCode.Grapin) as GameInputButton).GetState().heldDown);
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.IsHooked)
        {
            D = (transform.position - Player.transform.position);
            grapin.transform.position = transform.position - D / 2;
            grapin.transform.localScale = new Vector3(1, D.magnitude / 2, 1) / 10;
            grapin.transform.rotation = Quaternion.FromToRotation(transform.up, D);
        }
    }
}
