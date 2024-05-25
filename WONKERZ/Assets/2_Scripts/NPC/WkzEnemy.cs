using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using Schnibble;
using Schnibble.AI;
using Schnibble.Rendering;
using static UnityEngine.Debug;

namespace Wonkerz
{

    public class WkzEnemy : WkzNPC
    {
        [Header("# WkzEnemy\nMAND")]
        public GameObject playerSpottedEffect;

        [Header("Tweaks")]
        public float spottedMarkerDuration = 3f;
        public float dynamicAtkDecalYOffset = 0f;
        [Header("Optionals")]

        public GameObject dynamicAttackDecal_Ref;
        protected GameObject dynamicAttackDecal_Inst;

        public List<GameObject> staticAtkDecal_Refs;


        protected IEnumerator ShowSpottedMarker(WkzEnemy iSelf)
        {
            playerSpottedEffect.SetActive(true);
            float show_elapsed = 0f;
            float anim_freq = 1 / iSelf.spottedMarkerDuration;

            while (show_elapsed < iSelf.spottedMarkerDuration)
            {
                show_elapsed += Time.deltaTime;
                yield return null;
            }

            playerSpottedEffect.SetActive(false);
        }

        public void facePlayer(bool iForceNoLERP = false)
        {
            faceTarget(Access.Player().transform.position, iForceNoLERP);
        }

        public void faceTarget(Vector3 iTarget, bool iForceNoLERP = false)
        {
            Vector3 target = iTarget - transform.position;
            float rotSpeed = (iForceNoLERP) ? 180f : 2f * Time.deltaTime;
            float maxRotMagDelta = 0f;

            Vector3 from = transform.forward;
            from.y = 0f;

            Vector3 to = target;
            to.y = 0f;

            //Vector3 newDir = Vector3.RotateTowards(transform.forward, target, rotSpeed, maxRotMagDelta);
            Vector3 newDir = Vector3.RotateTowards(from, to, rotSpeed, maxRotMagDelta);
            transform.rotation = Quaternion.LookRotation(newDir);
        }

        public virtual void kill() { }

        protected void showStaticAtkDecals(bool iState)
        {
            foreach (GameObject decal in staticAtkDecal_Refs)
            {
                decal.SetActive(iState);
            }
        }

        public void toggleOffDynamicDecal()
        {
            spawnDynamicAtkDecal(false, Vector3.zero);
        }

        public void toggleOnDynamicDecal(Vector3 iTarget)
        {
            spawnDynamicAtkDecal(true, iTarget);
        }

        protected void spawnDynamicAtkDecal(bool iState, Vector3 iTarget)
        {
            if (dynamicAttackDecal_Ref == null)
            return;

            if (!iState)
            {
                if (dynamicAttackDecal_Inst != null)
                {
                    Destroy(dynamicAttackDecal_Inst.gameObject);
                }
            }
            else
            {

                Vector3 targetPos = iTarget;
                Vector3 selfPos = transform.position;

                Vector3 decalPosition = Vector3.Lerp(targetPos, selfPos, 0.5f); // halfway
                decalPosition.y = 0f;


                if (dynamicAttackDecal_Inst == null)
                {
                    dynamicAttackDecal_Inst = Instantiate(dynamicAttackDecal_Ref);
                    dynamicAttackDecal_Inst.transform.parent = transform;
                    dynamicAttackDecal_Inst.transform.rotation = transform.rotation;
                }

                dynamicAttackDecal_Inst.transform.position = new Vector3(decalPosition.x, 0f, decalPosition.z);
                dynamicAttackDecal_Inst.transform.localPosition = new Vector3(dynamicAttackDecal_Inst.transform.localPosition.x, dynamicAtkDecalYOffset, dynamicAttackDecal_Inst.transform.localPosition.z);
                // Todo : use transform point instead to have a better Y offset during aiming phase (so the decal doesn't flicker cuz of Y coord)
                //dynamicAttackDecal_Inst.transform.position = dynamicAttackDecal_Inst.transform.TransformPoint(decalPosition.x, atkChargeDecalYOffset, decalPosition.z);

                dynamicAttackDecal_Inst.transform.localScale = new Vector3(
                    dynamicAttackDecal_Inst.transform.localScale.x,
                    dynamicAttackDecal_Inst.transform.localScale.y,
                    Mathf.Ceil(Vector3.Distance(targetPos, selfPos))
                );
            }
        }

        protected void tryUnparentAttackDecal()
        {
            if (dynamicAttackDecal_Inst == null)
            return;
            dynamicAttackDecal_Inst.transform.parent = null;
        }
    }
}
