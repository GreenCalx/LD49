using UnityEngine;

public class BallLiftTrap : Trap
{
    [Header("Mandatory")]
    public Transform spawnPoint;
    public GameObject ballRef;
    public Transform millTransform;

    [Header("Tweaks")]
    public float rotSpeed = 5f;
    ///


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void OnTrigger()
    {
        GameObject newball = Instantiate(ballRef);
        newball.transform.position = spawnPoint.position;
        Resetable ballResetable = newball.GetComponent<Resetable>();
        ballResetable.segmentMCP = ParentMCP;
    }

    public override void OnCharge(float iCooldownPercent)
    {
    }

    public override void OnRest(float iCooldownPercent)
    {
        millTransform.RotateAround(millTransform.position, transform.forward, Time.deltaTime * 90);
    }
}
