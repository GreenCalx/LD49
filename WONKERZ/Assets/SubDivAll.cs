using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubDivAll : MonoBehaviour
{
    public void Bake () {
        foreach(var g in GameObject.FindObjectsOfType<subdiv>()){
            g.Bake();
        }
    }
}
