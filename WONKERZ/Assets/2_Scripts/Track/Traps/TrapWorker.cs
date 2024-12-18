using UnityEngine;

/**
* Indicates that implementing class can operate traps
**/
public abstract class TrapWorker : MonoBehaviour
{
    public Animator animator;

    public abstract void changeAnimatorBoolParm(string iParm, bool iVal);
}
