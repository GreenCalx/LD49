using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikedSphere : MonoBehaviour
{
    public int damageOnCollide = 2;

    void OnCollisionEnter(Collision iCol)
    {
        CarController cc = iCol.gameObject.GetComponent<CarController>();
        if (!!cc)
        {
            cc.takeDamage(damageOnCollide, iCol.contacts[0]);
        }
    }
}
