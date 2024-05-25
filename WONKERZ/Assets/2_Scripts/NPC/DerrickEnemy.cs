using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Schnibble.AI;

namespace Wonkerz
{

    public struct DerrickEnemyRelationship
    {
        public DerrickEnemy connectedDerrick;
        public bool isMaster;
        public GameObject ElectricityRay_Inst;
        public SpringJoint joint;
    }

    public class DerrickEnemy : WkzEnemy
    {
        [Header("# DerrickEnemy\nMAND")]
        public Mesh ElectricityRayMesh;
        public Material ElectricityRayMaterial;
        public Transform RayAnchor;

        [Header("Tweaks")]
        public float timeBetweenAction = 1f;
        public float pairing_distance = 50f;
        public readonly int connect_slots = 1; // TODO : dev correct strategies for slots>1 if needed
        public int rayDamage = 5;
        public int rayRepulseOnDamage = 10;
        public float jointMinDist = 30f;
        public float jointMaxDist = 80f;
        public float jointSpringForce = 12f;
        public float jointSpringDamper = 2f;

        [Header("Internals")]
        public List<DerrickEnemyRelationship> pairedDerricks = new List<DerrickEnemyRelationship>(0);
        //public GameObject ElectricityRay_Inst;
        private float idle_timer;

        void Start()
        {
            ai_init();
            pairedDerricks = new List<DerrickEnemyRelationship>(0);
            idle_timer = 0f;
        }

        void Update()
        {

        }


        protected override void OnAggro()
        {
            StartCoroutine(ShowSpottedMarker(this));
        }

        protected override void InAggro()
        {
            if (idle_timer < timeBetweenAction)
            {
                facePlayer();
                idle_timer += Time.deltaTime;
                return;
            }
            idle_timer = 0f;

            // Try to pair at least 1 derrick in priority if solo
            if (pairedDerricks.Count == 0)
            {
                TryFindPair();
                agent.SetDestination(GetNextPositionFromCoordinator());
                return;
            }

            //  slaves relative sweeps OR Masters Aim player
            if (pairedDerricks.Count == 1)
            {
                DerrickEnemyRelationship der = pairedDerricks[0];
                if (!der.isMaster)
                agent.SetDestination(GetNextRelativePositionFromCoordinator(der.connectedDerrick.transform));
                else
                    agent.SetDestination(GetNextPositionFromCoordinator());
            }

        }
        protected override void OutAggro()
        {
            // terminate rays

        }

        protected override void PreLaunchAction()
        {
            // nothing?
        }
        protected override void PreStopAction()
        {
            // nothing?
        }

        public override void kill()
        {
            foreach (DerrickEnemyRelationship der in pairedDerricks)
            {
                if (der.isMaster)
                {
                    Destroy(der.ElectricityRay_Inst);
                }
                der.connectedDerrick.UnpairWith(this);
            }

            facePlayer(true);
            ai_kill();

            Destroy(gameObject);
        }

        public void TryFindPair()
        {
            if (pairedDerricks.Count >= connect_slots)
            {
                return;
            }

            currStrategy = SchAIStrategies.AGENT_STRAT.AGGLOMERATE;

            foreach (SchAIAgent ai in coordinator.childs)
            {
                DerrickEnemy as_DE = ai.GetComponent<DerrickEnemy>();
                if (as_DE == this)
                continue;

                if (!!as_DE)
                {
                    bool isAlreadyConnectedTo = false;
                    foreach (DerrickEnemyRelationship der in pairedDerricks)
                    {
                        if (der.connectedDerrick == as_DE)
                        { isAlreadyConnectedTo = true; break; }
                    }
                    if (isAlreadyConnectedTo)
                    continue;

                    float dist = Vector3.Distance(transform.position, as_DE.transform.position);
                    if (dist < pairing_distance)
                    {
                        // eligible, check slots?
                        if (as_DE.pairedDerricks.Count < as_DE.connect_slots)
                        {
                            PairWith(as_DE);
                            return;
                        }
                    }
                }
            }
        }

        public void PairWith(DerrickEnemy iOtherDerrick)
        {
            // Set others derrick slave relationship with self first
            DerrickEnemyRelationship other_DER = new DerrickEnemyRelationship();
            other_DER.connectedDerrick = this;
            other_DER.isMaster = false;
            other_DER.ElectricityRay_Inst = null;
            other_DER.joint = null;
            iOtherDerrick.pairedDerricks.Add(other_DER);

            DerrickEnemyRelationship self_DER = new DerrickEnemyRelationship();
            self_DER.connectedDerrick = iOtherDerrick;
            self_DER.isMaster = true;

            //spawn visual ray
            self_DER.ElectricityRay_Inst = new GameObject("ElectricityRay:" + gameObject.name + ">" + iOtherDerrick.gameObject.name);
            MeshFilter mf = self_DER.ElectricityRay_Inst.AddComponent<MeshFilter>();
            MeshRenderer mr = self_DER.ElectricityRay_Inst.AddComponent<MeshRenderer>();
            ElectricityRay er = self_DER.ElectricityRay_Inst.AddComponent<ElectricityRay>();
            BoxCollider bc = self_DER.ElectricityRay_Inst.AddComponent<BoxCollider>();
            PlayerDamager pd = self_DER.ElectricityRay_Inst.AddComponent<PlayerDamager>();

            mf.sharedMesh = ElectricityRayMesh;
            mr.sharedMaterial = ElectricityRayMaterial;

            er.From = RayAnchor;
            er.To = iOtherDerrick.RayAnchor;

            pd.damageOnCollide = rayDamage;
            pd.optional_repulsion_force = rayRepulseOnDamage;

            bc.size = new Vector3(1, 2, 1);

            // Set up joint
            self_DER.joint = gameObject.AddComponent<SpringJoint>();
            self_DER.joint.connectedBody = iOtherDerrick.GetComponent<Rigidbody>();

            self_DER.joint.autoConfigureConnectedAnchor = true;
            self_DER.joint.anchor = transform.TransformPoint(RayAnchor.position);

            self_DER.joint.minDistance = jointMinDist;
            self_DER.joint.maxDistance = jointMaxDist;
            self_DER.joint.spring = jointSpringForce;
            self_DER.joint.damper = jointSpringDamper;
            self_DER.joint.enableCollision = true;

            pairedDerricks.Add(self_DER);

            // update strategies of both derricks
            currStrategy = SchAIStrategies.AGENT_STRAT.AIM_FOR;
            iOtherDerrick.currStrategy = SchAIStrategies.AGENT_STRAT.SWEEP_AROUND;

        }

        public void UnpairWith(DerrickEnemy iOtherDerrick)
        {
            foreach (DerrickEnemyRelationship der in pairedDerricks)
            {
                if (der.connectedDerrick == iOtherDerrick)
                {
                    pairedDerricks.Remove(der);
                    if (der.isMaster)
                    {
                        Destroy(der.ElectricityRay_Inst);
                        Destroy(der.joint);
                    }
                    return;
                }
            }
        }

    }
}
