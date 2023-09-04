using UnityEngine;

public class TorusRotater : MonoBehaviour
{
    public Vector3 rot_axis = new Vector3(1f, 0f, 0f);
    public float rot_speed = 2f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // rotate around forward
        transform.Rotate(rot_axis, rot_speed * Time.deltaTime);
    }
}
