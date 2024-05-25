using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Schnibble;

namespace Wonkerz
{
    public class EnemyDamageable : MonoBehaviour
    {
        [Header("MAND")]
        public WkzEnemy attachedEnemy;
        [Header("Tweaks")]
        public bool damageOnSpinOnly = true;
        public int hitpoints = 1;


        private int currHP;

        void Start()
        {
            currHP = hitpoints;
        }

        void OnCollisionEnter(Collision iCol)
        {
            if (damageOnSpinOnly)
            {
                SpinPowerObject spo = iCol.gameObject.GetComponent<SpinPowerObject>();
                if (!!spo)
                {
                    currHP--;
                }
            }
            else
            {
                currHP--;
            }

            if (currHP <= 0)
            {
                attachedEnemy.kill();
            }
        }
    }
}
