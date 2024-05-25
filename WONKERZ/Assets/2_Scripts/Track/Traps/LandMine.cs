using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wonkerz
{
    public class LandMine : Bomb
    {
        [Header("ProximityMine")]
        public bool isProximityMine = true;
        public WonkerDecal zoneDecal;
        public PlayerDetector playerDetector;
        public Texture2D triggeredColorRamp;
        [Header("Refs")]
        public MeshDeform attachedGroundToDeform;

        public float timeBeforeExplosion = 1f;
        private float elapsedTimeBeforeExplosion = 0f;

        private bool isTriggered = false;

        private Vector3 initZoneDecalScale;

        // Start is called before the first frame update
        void Start()
        {
            if (!!zoneDecal)
            initZoneDecalScale = zoneDecal.transform.localScale;
            elapsedTimeBeforeExplosion = 0f;
            isTriggered = false;
            if (isProximityMine)
            updateLandMineColor();
        }

        // Update is called once per frame
        void Update()
        {

            if (isProximityMine)
            {
                isTriggered = playerDetector.playerInRange;

                if (isTriggered)
                {
                    if (elapsedTimeBeforeExplosion > timeBeforeExplosion)
                    {
                        explode();
                        movePoints();
                    }
                    elapsedTimeBeforeExplosion += Time.deltaTime;
                    updateLandMineColor();
                }
                else
                {
                    if (elapsedTimeBeforeExplosion > 0f)
                    {
                        elapsedTimeBeforeExplosion -= Time.deltaTime;
                        elapsedTimeBeforeExplosion = Mathf.Clamp(elapsedTimeBeforeExplosion, 0f, timeBeforeExplosion);
                        updateLandMineColor();
                    }
                }

            }
        }

        public void movePoints()
        {
            Vector3[] otherVerts = attachedGroundToDeform.originalVertices;
            Vector3 localColPos = transform.InverseTransformPoint(transform.position);

            float distance;
            for (int i = 0; i < otherVerts.Length; i++)
            {
                distance = Vector3.Distance(transform.position, attachedGroundToDeform.transform.TransformPoint(otherVerts[i]));
                if (distance <= explosionRange)
                {
                    attachedGroundToDeform.StartDisplacement(otherVerts[i]);
                }
            }
            //attachedGroundToDeform.UpdateMesh(otherVerts);

        }

        private void updateLandMineColor()
        {
            if (triggeredColorRamp == null)
            return;

            float width = triggeredColorRamp.width;
            float time_ratio = Mathf.Clamp(elapsedTimeBeforeExplosion / timeBeforeExplosion, 0f, 1f);

            Color newcolor = triggeredColorRamp.GetPixel((int)(time_ratio * width), 0);
            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (!!mr)
            {
                mr.material.SetColor("_Color", newcolor);
            }


            Vector3 zoneDecalScale = new Vector3(initZoneDecalScale.x * time_ratio, initZoneDecalScale.y * time_ratio, initZoneDecalScale.z * time_ratio);
            zoneDecal.transform.localScale = zoneDecalScale;
            zoneDecal.transform.localPosition = Vector3.zero;
            zoneDecal.SetAnimationTime(1f);
        }

    }
}
