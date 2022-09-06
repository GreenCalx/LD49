using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* Indicates that implementing class can operate traps
**/
public abstract class TrapWorker : MonoBehaviour
{
    public Animator animator;

    public float timeToLoadTrap;

    public float restTimeBetweenActivations;
    public abstract void changeAnimatorBoolParm(string iParm, bool iVal);
}