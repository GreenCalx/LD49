using Schnibble.Managers;
using UnityEngine;
using static Schnibble.Physics;
using Schnibble;
using System;

namespace Wonkerz
{

    public class ManualCamera : PlayerCamera, IControllable
    {
        //    '-.  angle
        //       '-.
        // C ------- Target
        //    length
        public struct CameraCone
        {
            public float angle;
            public float length;
        };

        public float resetSpeed          = 10.0f;
        public float orbitSpeedDegPerSec = 45.0f;
        public float distance            = 30.0f;
        public float dollyZoomMul = 1.0f;
        public float height              = 10.0f;
        public float lookAheadMul        = 1.0f;

        public bool autoAlign = true;

        public SchSpring positionDistanceSpring;

        bool locked = false;

        Vector3    targetPosition;
        Quaternion targetRotation;
        Vector3    focusPoint;
        Vector3    lastPosition;

        float distanceInitial = 0.0f;

        public override void applySpeedEffect(float iSpeedPerCent)
        {
            base.applySpeedEffect(iSpeedPerCent);
            distance = distanceInitial - iSpeedPerCent * dollyZoomMul;
        }

        public void forceAngles(bool iForce, Vector2 forceAngle)
        {
#if false
            autoAlign = iForce;
            if (!iForce) return;

            orbitAngles = forceAngle;
            //transform.localRotation = Quaternion.Euler(orbitAngles);
#endif
        }

        void OnDestroy()
        {
            try
            {
                Access.managers.playerInputsMgr.player1.Detach(this as IControllable);
            }
            #pragma warning disable CS0168
            catch (NullReferenceException e)
            {
                this.Log(gameObject.name + " OnDestroy : NULL ref on detachable");
            }
            #pragma warning restore CS0168
        }

        protected override void Awake()
        {
            base.Awake();
            distanceInitial = distance;
        }

        public override void init()
        {
            base.init();

            if (player==null)
                player = Access.Player();
            if (player) 
                playerRef = player.GetTransform().gameObject;

            Access.managers.playerInputsMgr.player1.Attach(this);

            resetView();
        }

        public override void resetView()
        {
            if (playerRef == null) return;
            if (positionDistanceSpring == null) return;
            // reset view behing player.
            locked = true;

            focusPoint = playerRef.transform.position;
            targetPosition = focusPoint - distance * playerRef.transform.forward + height * Vector3.up;
            positionDistanceSpring.Activate();
        }

        public bool AutomaticRotation(float dt)
        {
            var playerRB = playerRef.GetComponent<Rigidbody>();
            focusPoint = playerRef.transform.position;
            if (playerRB)
            {
                var playerVelocity = playerRB.velocity;

                focusPoint += lookAheadMul * playerRB.velocity;

                var dir = Vector3.Dot(playerVelocity, cam.transform.forward);
                if (dir > 0.0f)
                {
                    var diff = focusPoint - cam.transform.position;
                    var diffSize = diff.magnitude;
                    if (diffSize != 0.0f) diff /= diffSize;
                    else diff = Vector3.zero;

                    targetPosition = cam.transform.position + (diffSize - distance) * diff;
                }
                else
                {
                    var diff = focusPoint - cam.transform.position;
                    var diffSize = diff.magnitude;
                    if (diffSize != 0.0f) diff /= diffSize;
                    else diff = Vector3.zero;

                    targetPosition = cam.transform.position + (diffSize - distance) * diff;
                }

                targetPosition.y = focusPoint.y + height;

                Schnibble.Debug.DrawWireSphere(0.1f, focusPoint, 4, 4, Color.magenta);

                positionDistanceSpring.Activate();

                return true;
            }

            return false;
        }

        public bool ManualRotation(float dt)
        {
            // Manual camera : update from inputs.
            var input = frameInput.average;
            if ((input.x * input.x + input.y * input.y) == 0.0f) return false;

            var focusPoint = playerRef.transform.position;
            var forward = (focusPoint - cam.transform.position);
            var diff = forward.magnitude;
            if (diff != 0.0f) forward /= diff;
            else forward = Vector3.zero;

            var up = Vector3.up;

            targetRotation = Quaternion.LookRotation(forward, up);

            // TODO: take care of corner case.
            var dot = Vector3.Dot(forward, up);
            if (Mathf.Sign(dot) != Mathf.Sign(input.x) && Mathf.Abs(dot) >= 0.99f)
            {
                input.x = 0.0f;
            }
            // arclength = radius * angle.
            // radius is magnitude of forward with y = 0
            // hence we scale back the input.y by the radius of the normalized radius,
            // which in turn gives us a normalized input in term of arclength.
            var radius = Vector3.Scale(forward, new Vector3(1, 0, 1)).magnitude;
            var inputScaled = new Vector2(input.y * radius * orbitSpeedDegPerSec * dt, input.x * dt * orbitSpeedDegPerSec);

            targetRotation *= Quaternion.AngleAxis(inputScaled.x, Vector3.up) * Quaternion.AngleAxis(inputScaled.y, Vector3.right);
            targetPosition = focusPoint - targetRotation * Vector3.forward * diff;

            positionDistanceSpring.Deactivate();

            return true;
        }

        public void UpdateCameraPositionAndRotationFromTarget(Vector3 targetPosition, Quaternion targetRotation, float dt)
        {
            // Update distance first.
            Vector3 errorPosition = cam.transform.position - targetPosition;
            positionDistanceSpring.currentLength = errorPosition.magnitude;
            lastPosition = cam.transform.position;
            if (positionDistanceSpring.IsActive())
            {
                var error = errorPosition.magnitude;
                // unlock when we are at needed position.
                if (locked && error < 0.1f)
                {
                    locked = false;
                }

                if (error != 0.0f)
                {

                    var errorDir = errorPosition / error;
                    var vel = Vector3.Dot(lastPosition - cam.transform.position, errorDir);
                    var correction = -error * positionDistanceSpring.stiffness - vel * positionDistanceSpring.damp;

                    vel += correction * dt;
                    cam.transform.position += vel * errorDir;
                }
            }
            else
            {
                cam.transform.position = targetPosition;
            }

            // transform is not what we assumed, recompute rotation.
            var forward = (focusPoint - cam.transform.position);
            var up = Vector3.up;

            targetRotation = Quaternion.LookRotation(forward, up);
            cam.transform.rotation = targetRotation;
        }

        public void LateUpdate()
        {
            var dt = Time.deltaTime;
            // might not have fully loaded.
            if (playerRef == null) return;
            // Update directions.
            if (!HasFocus())
            {
                // Manual > automatic.
                focusPoint = playerRef.transform.position;

                var forward = (focusPoint - cam.transform.position);
                var up = Vector3.up;

                if (locked) resetView();
                else if (!locked && !ManualRotation(dt))
                {
                    AutomaticRotation(dt);
                }
            }
            else
            {
                focusPoint = secondaryFocus.transform.position;

                var focusDir = (secondaryFocus.transform.position - playerRef.transform.position).normalized;
                targetPosition = playerRef.transform.position - focusDir * distance + height * Vector3.up;
                positionDistanceSpring.Activate();
            }

            UpdateCameraPositionAndRotationFromTarget(targetPosition, targetRotation, dt);

            frameInput.Reset();
        }

        public bool StartFocus()
        {
            if (!findFocus()) return false;
            return true;
        }

        public bool StopFocus()
        {
            resetFocus();
            return true;
        }

        public bool CanFocus()
        {
            return !focusInputLock;
        }

        public bool CanSwitchFocus()
        {
            return HasFocus() && elapsedTimeFocusChange >= focusChangeInputLatch;
        }

        public bool HasFocus()
        {
            return secondaryFocus != null;
        }

        Schnibble.Math.AccumulatorV2 frameInput;
        void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
        {
            if (!HasFocus())
            {
                // input is cameraY, cameraX, because it represents the axis of rotation.
                // therefor, trying to move the camera left (cameraX) means rotating around Y orbitaly.
                Vector2 multiplier = new Vector2(InputSettings.InverseCameraMappingX ? -1f : 1f, InputSettings.InverseCameraMappingY ? -1f : 1f);
                var current = new Vector2(
                    (Entry.Get((int)PlayerInputs.InputCode.CameraY) as GameInputAxis).GetState().valueRaw,
                    -(Entry.Get((int)PlayerInputs.InputCode.CameraX) as GameInputAxis).GetState().valueRaw);

                frameInput.Add(current);
            }

            // View reset
            if ((Entry.Get((int)PlayerInputs.InputCode.CameraReset) as GameInputButton).GetState().down)
            {
                resetView();
            }

            // Camera targeting
            // set secondary focus
            var focusInput = (Entry.Get((int)PlayerInputs.InputCode.CameraFocus) as GameInputButton).GetState();

            if (focusInputLock) focusInputLock = !focusInput.up;

            if (CanFocus())
            {
                if (focusInput.down && HasFocus()) StopFocus();
                else if (focusInput.down) StartFocus();

                if (CanSwitchFocus())
                {
                    var focus_change_val = ((Entry.Get((int)PlayerInputs.InputCode.CameraFocusChange) as GameInputAxis)).GetState().valueRaw;
                    if (focus_change_val != 0.0f) changeFocus();
                }
            }
        }
    }


}
