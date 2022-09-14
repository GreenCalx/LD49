using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Spring : MonoBehaviour
{
    /// NOTE toffa : This is a spring damper system based on Hooke's law of elasticity.
    /// The inputs are a Frequency(F) and a Damping ratio(Dr). The spring Stiffness(k) and Damping value(Dv) will be deduced from it.
    /// IMPORTANT toffa : We dont take into account mass in our calculations. => m=1
    /// Dr = Dv / 2*sqr(m*k) <=> DV = 2*Dr*sqr(k)
    /// F = sqr(k/m) / 2*pi <=> k = (2*pi*F)²
    /// It could be directly set too if necessary.
    /// Then the spring will always try to get back to the rest position :
    /// Fs = force to apply
    /// L = length
    /// v = velocity
    /// Fs = -k*(Lrest - Lcurrent) - c*v
    ///
    /// Note that (Lrest - Lcurrent) could be seen as the "error" to correct, the formula is then :
    /// Fs = -error*k - c*v

    /// Note that MinLength < RestLength < MaxLength
    public float rest;
    public float max;
    public float min;
    /// Frequency <=> Stiffness
    public float f;
    // public float stiffness;
    /// DampRatio <=> DampValue
    public float dr;
    //public float dampValue;
    private float vel;
    private float position;

    public UnityEvent<float, Spring> onValueChange;

    public float ComputeSpring(float dt)
    {
        var error = position - rest;
        var force = -(f * error) - (dr * vel);

        vel += force;
        position += vel * dt;

        return position;
    }

    // from value space to spring space
    public float ToSpace(float min, float max, float value)
    {
        return Mathf.LerpUnclamped(this.min, this.max, (value - min) / (max - min));
    }

    // from value space to spring space
    public float FromSpace(float min, float max, float value)
    {
        return Mathf.LerpUnclamped(min, max, (value - this.min) / (this.max - this.min));
    }


    // Update is called once per frame
    void Update()
    {
        onValueChange.Invoke(ComputeSpring(Time.unscaledDeltaTime), this);
    }
}
