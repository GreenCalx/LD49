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
                Access.PlayerInputsManager().player1.Detach(this as IControllable);
            }
            catch (NullReferenceException e)
            {
                this.Log(gameObject.name + " OnDestroy : NULL ref on detachable");
            }
        }

        public override void Awake()
        {
            base.Awake();
            distanceInitial = distance;
        }

        public override void init()
        {
            base.init();

            player = Access.Player();
            if (player) playerRef = player.GetTransform().gameObject;

            Access.PlayerInputsManager().player1.Attach(this);

            resetView();
        }

        public override void resetView()
        {
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
                    diff /= diffSize;

                    targetPosition = cam.transform.position + (diffSize - distance) * diff;
                }
                else
                {
                    var diff = focusPoint - cam.transform.position;
                    var diffSize = diff.magnitude;
                    diff /= diffSize;

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
            forward /= diff;
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
            if (positionDistanceSpring.IsActive()) {
                var error      = errorPosition.magnitude;
                // unlock when we are at needed position.
                if (locked && error < 0.1f) {
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
            } else {
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

            UpdateCameraPositionAndRotationFromTarget(targetPosition, targetRotation, dt);

            frameInput.Reset();
        }

        public bool StartFocus()
        {
            if (!findFocus()) return false;

            //distance += camDistIncrease;
            //focusInputLock = true;
            return true;
        }

        public bool StopFocus()
        {
            resetFocus();
            //distance -= camDistIncrease;
            //focusInputLock = true;
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
                else StartFocus();

                if (CanSwitchFocus())
                {
                    var focus_change_val = ((Entry.Get((int)PlayerInputs.InputCode.CameraFocusChange) as GameInputAxis)).GetState().valueRaw;
                    if (focus_change_val != 0.0f) changeFocus();
                }
            }
        }
    }

#if false
    public class ManualCamera : PlayerCamera, IControllable
    {
        [Header("ManualCamera")]
        /// TWEAKS
        [SerializeField] public bool needButtonPressBeforeMove = true;
        [SerializeField] public bool autoAlign = false;
        [SerializeField] public Transform focus = default;
        [SerializeField, Range(1f, 80f)] public float distance = 5f;
        [SerializeField, Min(0f)] public float focusRadius = 1f;
        [SerializeField, Range(0f, 1f)] public float focusCentering = 0.5f;
        [SerializeField, Range(1f, 360f)] public float rotationSpeed = 90f;

        [SerializeField, Range(-89f, 89f)] public float minVerticalAngle = -30f, maxVerticalAngle = 60f, defaultVerticalAngle = 30f;
        [SerializeField, Min(0f)] public float alignDelay = 5f;
        [SerializeField, Min(0f)] public float alignDelayWithSecondaryFocus = 0f;
        [SerializeField, Range(0f, 90f)] float alignSmoothRange = 45f;
        [SerializeField] LayerMask obstructionMask = -1;
        [SerializeField, Range(0f, 180f)] public float camReverseDetectionThreshold = 140f;
        [Header("Jump")]
        [SerializeField, Min(0f)] float jumpDelay = 5f;
        [SerializeField, Min(0f)] float jumpMaxFocusRadius = 15f;
        [SerializeField, Min(0f)] float jumpFocusRadiusStep = 1f;
        /// Internals
        private Vector3 focusPoint, previousFocusPoint;
        private float previousHeadingAngle;
        private Vector2 orbitAngles = new Vector2(45f, 0f);
        [HideInInspector]
        public float lastManualRotationTime;
        private float jumpStartTime;
        private float baseFocusRadius;

        private Vector3 CameraHalfExtends
        {
            get
            {
                Vector3 halfExtends;
                halfExtends.y =
                    cam.nearClipPlane * Mathf.Tan(0.5f * Mathf.Deg2Rad * cam.fieldOfView);
                halfExtends.x = halfExtends.y * cam.aspect;
                halfExtends.z = 0f;
                return halfExtends;
            }
        }

        /// Methods
        void Awake()
        {
            cam = GetComponent<Camera>();
            //Utils.attachControllable<ManualCamera>(this);

            initial_FOV = cam.fieldOfView;
            jumpStartTime = 0f;
            baseFocusRadius = focusRadius;

        }
        private void Start()
        {
            init();
        }

        void Update()
        {
            updateSecondaryFocus();
            if (focus != null)
            DrawRay(focus.position, focus.forward * 10, Color.blue);
        }

        void OnDestroy()
        {
            try
            {
                Access.PlayerInputsManager().player1.Detach(this as IControllable);
            }
            catch (NullReferenceException e)
            {
                this.Log(gameObject.name + " OnDestroy : NULL ref on detachable");
            }
        }

        void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
        {
            // Camera manual movements if no focus
            if (null == secondaryFocus)
            {
                Vector2 multiplier = new Vector2(InputSettings.InverseCameraMappingX ? -1f : 1f,
                    InputSettings.InverseCameraMappingY ? -1f : 1f);
                if (!needButtonPressBeforeMove || Input.GetMouseButton(0))
                {
                    // input is cameraY, cameraX, because it represents the axis of rotation.
                    // therefor, trying to move the camera left (cameraX) means rotating around Y orbitaly.
                    var current = new Vector2(
                        (Entry.Get((int)PlayerInputs.InputCode.CameraY) as GameInputAxis).GetState().valueRaw,
                        -(Entry.Get((int)PlayerInputs.InputCode.CameraX) as GameInputAxis).GetState().valueRaw);
                    current = Vector2.Scale(current, multiplier);
                    input.Add(current);
                }
            }

            // View reset
            if ((Entry.Get((int)PlayerInputs.InputCode.CameraReset) as GameInputButton).GetState().down)
            {
                resetView();
            }

            // Camera targeting
            // set secondary focus
            if (focusInputLock)
            {
                if ((Entry.Get((int)PlayerInputs.InputCode.CameraFocus) as GameInputButton).GetState().up)
                {
                    focusInputLock = false;
                }
            }
            else
            { // No input lock
                // Y Pressed
                if ((Entry.Get((int)PlayerInputs.InputCode.CameraFocus) as GameInputButton).GetState().heldDown)
                {
                    // start focus
                    if (null == secondaryFocus)
                    {
                        findFocus();
                        if (null == secondaryFocus)
                        {
                            // have not found anything to focus, do not apply.
                        }
                        else
                        {
                            distance += camDistIncrease;
                            focusInputLock = true;
                        }
                    }
                    else // stop focus
                    {
                        resetFocus();
                        distance -= camDistIncrease;
                        focusInputLock = true;
                    }
                }
            }

            elapsedTimeFocusChange += Time.deltaTime;
            if ((null != secondaryFocus) && (elapsedTimeFocusChange >= focusChangeInputLatch))
            {
                var focus_change_val = ((Entry.Get((int)PlayerInputs.InputCode.CameraFocusChange) as GameInputAxis)).GetState().valueRaw;
                if (focus_change_val > 0)
                {
                    changeFocus();
                    elapsedTimeFocusChange = 0f;
                }
                else if (focus_change_val < 0)
                {
                    changeFocus();
                    elapsedTimeFocusChange = 0f;
                }
            }

        }

        public override void init()
        {
            var player = Access.Player();
            if (playerRef == null)
            {
                playerRef = player.GetTransform().gameObject;
            }

            if (focus == null)
            {
                player.inputMgr.Attach(this as IControllable);
            }
            focus = playerRef.transform;
            focusPoint = focus.position;

            resetView();
            //orbitAngles = new Vector2(defaultVerticalAngle, 180f);
            //transform.localRotation = Quaternion.Euler(orbitAngles);
        }

        public override void resetView()
        {
            Vector2 fwd_angle = (focus.position + focus.forward).normalized * -1;
            float headingAngle = GetAngle(fwd_angle);
            previousHeadingAngle = headingAngle;
            //orbitAngles.y = headingAngle;
            orbitAngles.y = focus.eulerAngles.y;
            orbitAngles.x = defaultVerticalAngle;
            //constrainAngles();

            Quaternion lookRotation = Quaternion.Euler(orbitAngles);
            Vector3 lookDirection = lookRotation * focus.forward;
            Vector3 lookPosition = focusPoint - lookDirection * distance;

            Vector3 rectOffset = lookDirection * cam.nearClipPlane;
            Vector3 rectPosition = lookPosition + rectOffset;
            Vector3 castFrom = focus.position;
            Vector3 castLine = rectPosition - castFrom;
            float castDistance = castLine.magnitude;

            Vector3 castDirection = castLine / castDistance;

            // check if we hit something between camera and focuspoint
            if (BoxCast(castFrom, CameraHalfExtends, castDirection, out RaycastHit hit, lookRotation, castDistance, obstructionMask))
            {
                rectPosition = castFrom + castDirection * hit.distance;
                lookPosition = rectPosition - rectOffset;
            }
            transform.SetPositionAndRotation(lookPosition, lookRotation);
        }

        // behavior

        void LateUpdate()
        {
            var player = Access.Player();
            if (!!player)
            {
                if (player.flags[PlayerController.FJump])
                {
                    if (jumpStartTime <= 0f)
                    jumpStartTime = Time.unscaledTime;
                    UpdateFocusPointInJump();
                }
                else
                {
                    jumpStartTime = 0f;
                    UpdateFocusPoint();
                }
            }
            else
            {
                UpdateFocusPoint();
            }

            //UpdateFocusPoint();
            Quaternion lookRotation;

            if (ManualRotation() || (autoAlign && autoRotation()))
            {
                constrainAngles();
                lookRotation = Quaternion.Euler(orbitAngles);
            }
            else
            {
                lookRotation = transform.localRotation;
            }

            Vector3 lookDirection = lookRotation * Vector3.forward;
            Vector3 lookPosition = focusPoint - lookDirection * distance;

            Vector3 rectOffset = lookDirection * cam.nearClipPlane;
            Vector3 rectPosition = lookPosition + rectOffset;
            Vector3 castFrom = focus.position;
            Vector3 castLine = rectPosition - castFrom;
            float castDistance = castLine.magnitude;

            Vector3 castDirection = castLine / castDistance;

            // check if we hit something between camera and focuspoint
            if (BoxCast(castFrom, CameraHalfExtends, castDirection, out RaycastHit hit, lookRotation, castDistance, obstructionMask))
            {
                rectPosition = castFrom + castDirection * hit.distance;
                lookPosition = rectPosition - rectOffset;
            }

            // align with secondary focus
            if (!!secondaryFocus)
            {

            }

            transform.SetPositionAndRotation(lookPosition, lookRotation);
        }

        void UpdateFocusPoint()
        {
            previousFocusPoint = focusPoint;

            Vector3 targetPoint = focus.position;

            if (focusRadius > baseFocusRadius)
            focusRadius = ((focusRadius - jumpFocusRadiusStep) > baseFocusRadius) ? focusRadius - jumpFocusRadiusStep : baseFocusRadius;

            if (focusRadius > 0f)
            {
                float distance = Vector3.Distance(targetPoint, focusPoint);
                float t = 1f;
                if (distance > 0.01f && focusCentering > 0f)
                { t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime); }
                if (distance > focusRadius)
                {
                    t = Mathf.Min(t, focusRadius / distance);
                }
                focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
            }
            else
            {
                focusPoint = targetPoint;
            }
        }

        void UpdateFocusPointInJump()
        {
            if (Time.unscaledTime - jumpStartTime < jumpDelay)
            return;

            previousFocusPoint = focusPoint;
            Vector3 targetPoint = focus.position;

            if (focusRadius < jumpMaxFocusRadius)
            focusRadius += jumpFocusRadiusStep;

            if (focusRadius > 0f)
            {
                float distance = Vector3.Distance(targetPoint, focusPoint);
                float t = 1f;
                if (distance > 0.01f && focusCentering > 0f)
                { t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime); }
                if (distance > focusRadius)
                {
                    t = Mathf.Min(t, focusRadius / distance);
                }
                focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
            }
            else
            {
                focusPoint = targetPoint;
            }
        }

        void OnValidate()
        {
            if (maxVerticalAngle < minVerticalAngle)
            {
                maxVerticalAngle = minVerticalAngle;
            }
        }

        void constrainAngles()
        {
            orbitAngles.x = Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);
            if (orbitAngles.y < 0f)
            {
                orbitAngles.y += 360f;
            }
            else if (orbitAngles.y >= 360f)
            {
                orbitAngles.y -= 360f;
            }
        }


        private Schnibble.Math.AccumulatorV2 input;
        bool ManualRotation()
        {
            const float e = 0.001f;

            var inputAvg = input.average;
            if (inputAvg.x < -e || inputAvg.x > e || inputAvg.y < -e || inputAvg.y > e)
            {
                orbitAngles += rotationSpeed * Time.unscaledDeltaTime * inputAvg;
                lastManualRotationTime = Time.unscaledTime;

                input.Reset();
                return true;
            }
            return false;
        }

        bool autoRotation()
        {
            if (null == secondaryFocus)
            if (Time.unscaledTime - lastManualRotationTime < alignDelay)
            return false;
            else
            if (Time.unscaledTime - lastManualRotationTime < alignDelayWithSecondaryFocus)
            return false;

            Vector2 movement = Vector2.zero;
            if (null == secondaryFocus)
            {
                movement = new Vector2(focusPoint.x - previousFocusPoint.x, focusPoint.z - previousFocusPoint.z);
            }
            else
            {
                movement = new Vector2(secondaryFocus.transform.position.x - focusPoint.x, secondaryFocus.transform.position.z - focusPoint.z);
            }



            float movementDeltaSqr = movement.sqrMagnitude;
            if (movementDeltaSqr < 0.000001f)
            {
                return false;
            }

            float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));

            if (null == secondaryFocus)
            {
                if (!ValidateNewHeadingAngle(headingAngle))
                {
                    return false;
                }
            }

            previousHeadingAngle = headingAngle;
            float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
            float rotationChange = rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
            if (deltaAbs < alignSmoothRange)
            {
                rotationChange *= deltaAbs / alignSmoothRange;
            }
            else if (180f - deltaAbs < alignSmoothRange)
            {
                rotationChange *= (180f - deltaAbs) / alignSmoothRange;
            }

            orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);
            return true;
        }

        static float GetAngle(Vector2 direction)
        {
            float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
            // (x < 0) => counterclockwise
            return direction.x < 0f ? 360f - angle : angle;
        }

        public void forceAlignToHorizontal(float iHorAngle)
        {
            orbitAngles = new Vector2(defaultVerticalAngle, iHorAngle);
            transform.localRotation = Quaternion.Euler(orbitAngles);

        }

        public void forceAngles(bool iForce, Vector2 forceAngle)
        {
            autoAlign = iForce;
            if (!iForce)
            return;
            orbitAngles = forceAngle;
            //transform.localRotation = Quaternion.Euler(orbitAngles);
        }

        public bool ValidateNewHeadingAngle(float iNewHeadingAngle)
        {
            // We don't want camera to go in front of player when he is going backward
            // Thus we don't accept any new angle with a delta around 180~
            // For now its not working well
            // TODO : Use Vector3.SignedAngle for a better result
            float deltaAngle = Mathf.DeltaAngle(iNewHeadingAngle, previousHeadingAngle);
            if ((deltaAngle >= camReverseDetectionThreshold) || (deltaAngle <= -camReverseDetectionThreshold))
            {
                return false;
            }

            return true;
        }
    }
#endif
}
