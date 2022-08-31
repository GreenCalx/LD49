using UnityEngine;

public class CameraFieldCleaner : MonoBehaviour
{
    public GameObject Player;
    public GameObject Camera;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var Colliders = Physics.OverlapBox(Camera.transform.position, Camera.transform.position - Player.transform.position, Camera.transform.rotation);
        foreach (var Collider in Colliders)
        {
            if (Collider.gameObject != Player) Collider.gameObject.SetActive(false);
        }
    }
}
