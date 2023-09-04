using UnityEngine;

public class CameraPoint : AbstractCameraPoint
{

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider iCollider)
    {

    }

    void OnTriggerStay(Collider iCollider)
    {
        Access.CheckPointManager().last_camerapoint = this;
    }

    void OnTriggerExit(Collider iCollider)
    {

    }
}
