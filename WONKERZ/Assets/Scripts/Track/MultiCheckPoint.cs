using System.Collections.Generic;
using UnityEngine;

public class MultiCheckPoint : MonoBehaviour
{
    public CheckPoint[] childs;
    public bool triggered;
    public string checkPointName = "";

    public List<Resetable> resetables = new List<Resetable>();

    // Start is called before the first frame update
    void Start()
    {
        childs = GetComponentsInChildren<CheckPoint>();
        triggered = false;
        foreach (CheckPoint cp in childs)
        {
            cp.checkpoint_name = checkPointName;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    // expected to be called by Resetables themselves
    public void addResetable(Resetable iR)
    {
        resetables.Add(iR);
    }

}
