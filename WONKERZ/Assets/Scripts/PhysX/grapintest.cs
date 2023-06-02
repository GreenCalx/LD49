using UnityEngine;
using Schnibble;

public class grapintest : MonoBehaviour, IControllable
{
    public PlayerController Player;
    public GameObject grapin;
    public Vector3 D;
    // Start is called before the first frame update
    void Start()
    {
        Utils.attachControllable<grapintest>(this);
    }

    private void OnDestroy()
    {
        Utils.detachControllable<grapintest>(this);
    }

    void IControllable.ProcessInputs(InputManager.InputData Entry)
    {
        Player.IsHooked = Entry.Inputs["Grapin"].Down;
        grapin.SetActive(Entry.Inputs["Grapin"].Down);
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
