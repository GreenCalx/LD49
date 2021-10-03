using UnityEngine;
using System;

public class CheckPoint : MonoBehaviour
{
    [System.Serializable]
    public class CameraDescriptor
    {
        public Vector3 position;
        public Quaternion rotation;

    };

    public CameraDescriptor CamDescEnd;
    public CameraDescriptor CamDescStart;
    public Camera Cam;
    private bool hasManager = false;
    public Transform respawn_location;
    [HideInInspector]
    public CheckPointManager cpm;
    public string name;

    public ParticleSystem particleSystem;
    public ParticleSystemForceField activation_pff;
    public ParticleSystemForceField base_pff;
    // Start is called before the first frame update
    void Start()
    {
        hasManager = subscribeToManager();
        activation_pff.gameObject.active = false;
        base_pff.gameObject.active = true;
    }


    // Update is called once per frame
    void Update()
    {
        if (!hasManager)
            hasManager = subscribeToManager();

        if (Cam)
        {
            Cam.transform.position = Vector3.Lerp(CamDescStart.position, CamDescEnd.position, Time.deltaTime);
        }


    }

    public bool subscribeToManager()
    {
        GameObject manager = GameObject.Find(Constants.GO_CPManager);
        cpm = manager.GetComponent<CheckPointManager>();
        if (!!cpm)
        {
            cpm.subscribe(this.gameObject);
            return true;
        }
        return false;
    }

    void OnTriggerEnter(Collider iCol)
    {
        CarController player = iCol.GetComponent<CarController>();
        if (!!player)
        {
            cpm.notifyCP(this.gameObject);
            activation_pff.gameObject.active = true;
            base_pff.gameObject.active = false;

        }
    }

    public GameObject getSpawn()
    {
        return respawn_location.gameObject;
    }

}
