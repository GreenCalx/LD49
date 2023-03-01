using UnityEngine;

public abstract class PIDController : MonoBehaviour
{
    public abstract PID GetController();
    public abstract void SetTarget(int index);
    public abstract float Power { get; set; }
}
