
public class LampiWorker : TrapWorker
{

    public bool isCheerLeader = false;
    private readonly string AnimIsCheerleader = "isCheerLeader";

    // Start is called before the first frame update
    void Start()
    {
        changeAnimatorBoolParm(AnimIsCheerleader, isCheerLeader);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void changeAnimatorBoolParm(string iParm, bool iVal)
    {
        if (!!animator)
        {
            animator.SetBool(iParm, iVal);
        }
    }
}
