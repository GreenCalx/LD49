using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble.SIUnits;
using static Schnibble.Utils;
using Schnibble;

namespace Schnibble
{
    public class SchGliderCollider : MonoBehaviour
    {
        public Rigidbody rb;

        public float maxAccelAngle;
        public float accel;

        public float maxBrakeAngle;
        public float brakeLeft;
        public float brakeRight;

        public Transform right;
        public Transform left;
        public Transform rightAttach;
        public Transform leftAttach;

        public float rhoSV;
        public Vector3 windSpeed;

        [System.Serializable]
        public struct WingValues
        {
            public Vector3 position;
            public Vector3 liftDir;
            public Vector3 dragDir;
            public float alpha;
            public float lift;
            public float drag;
        };

        public WingValues wingRight;
        public WingValues wingLeft;

        // todo: replace with curves
        float GetLiftCoeff(float angle) {
            return liftCoeff.Evaluate(angle);


            if (angle > 30)
            {
                // very high angle, not that much force.
                return 1;
            }
            return 10;
        }

        public AnimationCurve dragCoeff;
        public AnimationCurve liftCoeff;

        float GetDragCoeff(float angle) {
            return dragCoeff.Evaluate(angle);


            var maxDrag = 10;
            var minDrag = 1;
            return Mathf.Lerp(minDrag, maxDrag, angle / 90);
        }

        WingValues GetWingValues(Transform t, float brake) {
            WingValues res = new WingValues();
            var dt = Time.fixedDeltaTime;
            var velocity = rb.GetPointVelocity(t.position);
            var relWind = -velocity + windSpeed;

            res.position = t.position;
            res.dragDir = relWind.normalized;
            res.liftDir = Quaternion.AngleAxis(90, t.right) * res.dragDir;
            res.alpha = Vector3.Angle(velocity, t.forward);
            res.alpha += brake;
            res.alpha -= accel;
            // inv inertia
            Matrix4x4 R = Matrix4x4.Rotate(rb.inertiaTensorRotation); //rotation matrix created
            Matrix4x4 S = Matrix4x4.Scale(rb.inertiaTensor); // diagonal matrix created
            var inertiaTensor = R * S * R.transpose; // R is orthogonal, so R.transpose == R.inverse
            Matrix4x4 rotationMatrix = Matrix4x4.Rotate(rb.transform.rotation);
            var inertiaTensorWorld = rotationMatrix * inertiaTensor * rotationMatrix.transpose;
            var invI = inertiaTensorWorld.inverse;
            var unitImpLift = Physics.GetUnitImpulse(invI, 1.0f/rb.mass, res.position - rb.worldCenterOfMass, res.liftDir);
            var unitImpDrag = Physics.GetUnitImpulse(invI, 1.0f/rb.mass, res.position - rb.worldCenterOfMass, res.dragDir);

            var lift = rhoSV * (relWind.magnitude * relWind.magnitude) * GetLiftCoeff(res.alpha);
            var drag = rhoSV * (relWind.magnitude * relWind.magnitude) * GetDragCoeff(res.alpha);

            res.lift = lift;
            res.drag = drag;

            //res.lift = Vector3.Dot(relWind, res.liftDir) / unitImpLift / dt;
            //res.drag = Vector3.Dot(relWind, res.dragDir) / unitImpDrag / dt;

            return res;
        }

        void DrawDebugWing(WingValues w) {
            UnityEngine.Debug.DrawLine(w.position, w.position + w.liftDir, Color.blue);
            UnityEngine.Debug.DrawLine(w.position, w.position + w.dragDir, Color.red);
            UnityEngine.Debug.DrawLine(w.position, w.position + rb.GetPointVelocity(w.position).normalized, Color.yellow);
        }

                void ApplyForce(WingValues w ) {
                    rb.AddForceAtPosition(w.lift * w.liftDir + w.drag * w.dragDir, w.position);
                }

            void FixedUpdate()
            {
                wingRight = GetWingValues(right, brakeRight);
                wingLeft  = GetWingValues(left, brakeLeft);

                DrawDebugWing(wingRight);
                DrawDebugWing(wingLeft);

                ApplyForce(wingRight);
                ApplyForce(wingLeft);
            }
        }
    }
