using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class VaranBoss : MonoBehaviour
{
        public Transform t_boss;
        public Transform goal;

        public float base_speed;
        public float roll_speed;

        public float roll_rot_speed = 5f;
        private const string _roll_anim_parm = "ROLL";
        private Animator __animator;
        public bool is_rollin;
        private float init_x_rot;

       NavMeshAgent agent;

       void Start () {
            agent = GetComponent<NavMeshAgent>();
            __animator = GetComponent<Animator>();

            agent.destination = goal.position;
            is_rollin = false;

            if (t_boss == null)
            {
                Debug.LogError("Boss transform is missing!!");
            }

            agent.speed = base_speed;
            init_x_rot = t_boss.rotation.x;
       }

       void Update()
       {
          agent.destination = goal.position;

          if (is_rollin)
          {
              t_boss.Rotate( roll_rot_speed, 0f,0f,Space.Self);
          }

       }

        // if player is out of range
       void OnTriggerExit(Collider iCollider)
       {
           CarController cc = iCollider.GetComponent<CarController>();
           if (!!cc)
           {
               roll(true);
           }
       }

        //player in range, stop rolling
        void OnTriggerEnter(Collider iCollider)
       {
           CarController cc = iCollider.GetComponent<CarController>();
           if (!!cc)
           {
               roll(false);
           }
       }

       private void roll(bool iState)
       {
            is_rollin = iState;

            if (!!__animator)
                __animator.SetBool( _roll_anim_parm, is_rollin);

            agent.speed = is_rollin ? roll_speed : base_speed;

            // reset boss rotation when he stops rolling
            if (!is_rollin)
            {
                //float x_rot = t_boss.rotation.x;
                //t_boss.Rotate( 360 - x_rot, 0f, 0f, Space.Self );
            }

       }


}
