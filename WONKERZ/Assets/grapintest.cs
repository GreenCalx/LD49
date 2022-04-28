using UnityEngine;

public class grapintest : MonoBehaviour
{
    public CarController Player;
    public GameObject grapin;
    public Vector3 D;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.K))
        {
            Player.IsHooked = true;

            grapin.active = true;

            D = (transform.position - Player.transform.position);
            grapin.transform.position = transform.position - D / 2;
            grapin.transform.localScale = new Vector3(1, D.magnitude / 2, 1) / 10;
            grapin.transform.rotation = Quaternion.FromToRotation(transform.up, D);
        }
        if (Input.GetKeyUp(KeyCode.K))
        {
            Player.IsHooked = false;
            grapin.active = false;
        }
    }
}
