using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCPierre : NPCGaragist
{
    [Header("# NPCPierre")]

    [Header("Self References")]
    public Animator selfAnimator;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnFree()
    {
        // TODO : animate the fucker out.
        // Might need to also clean focusables
    }
}
