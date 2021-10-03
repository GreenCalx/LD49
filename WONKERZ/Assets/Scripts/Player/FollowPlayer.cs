using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject Following;
    public Vector3 Distance;
    public float LerpMult;
    public bool Active = true;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!Active) return;
        var FinalPosition = Following.transform.position + Distance;
        var MaxDistance = FinalPosition + (FinalPosition * LerpMult);
        var Lerp = 1 / (MaxDistance.sqrMagnitude / (transform.position - FinalPosition).sqrMagnitude);

        transform.position = Vector3.Lerp(transform.position, FinalPosition, Lerp);

        transform.LookAt(Following.transform.position);

    }
}
