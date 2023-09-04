
public class WheelWorkstation : TrapWorkstation
{
    // Start is called before the first frame update
    void Start()
    {

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
