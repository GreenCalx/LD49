using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Schnibble.SchPhysics;

public class SchRigidBody : MonoBehaviour
{
    public MonoBehaviour rb;
    public IRigidBody irb;
    void Start()
    {
        irb = rb as IRigidBody;
    }
}
