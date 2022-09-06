using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TrapWorkstation : MonoBehaviour
{
    public Animator animator;
    
    public abstract void changeAnimatorBoolParm(string iParm, bool iVal);
}
