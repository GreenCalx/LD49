using UnityEngine;

public class FollowPlayer : PlayerCamera
{
    public Vector3 Distance;
    public float LerpMult;
    public bool Active = true;
    public CheckPointManager CPM;
    // Start is called before the first frame update
    
    void Awake()
    {
        camType = CAM_TYPE.TRACK;
    }

    void Start()
    {

    }

    public override void init() 
    {
        playerRef = Utils.getPlayerRef();
        //CPM = FindObjectOfType<CheckPointManager>();
        CPM = GameObject.Find(Constants.GO_CPManager).GetComponent<CheckPointManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!Active) 
            return;
        if (!CPM)
            init();


        Distance = CPM.last_checkpoint.GetComponent<CheckPoint>().CamDescEnd.position;

        var FinalPosition = playerRef.transform.position + Distance.x * Vector3.right + Distance.y * Vector3.up + Distance.z * Vector3.forward;
        var FinalPositionDirection = playerRef.transform.position - FinalPosition;
        var MaxDistancePosition = (FinalPosition - (FinalPositionDirection * LerpMult));
        var MaxDistance = MaxDistancePosition.magnitude;

        var MaxDistanceMagn = (MaxDistancePosition - FinalPosition).magnitude;

        var CurrentDistance = (transform.position - FinalPosition).magnitude;

        var Lerp = (CurrentDistance / MaxDistanceMagn);

        transform.position = Vector3.Lerp(transform.position, FinalPosition, Lerp);

        transform.LookAt(playerRef.transform.position);

    }
}
