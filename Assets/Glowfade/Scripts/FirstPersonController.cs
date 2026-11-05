using UnityEngine;
using UnityEngine.InputSystem;

namespace Glowfade
{
    public enum Stance { Standing, Crouching, LyingDown, Sitting }

    /// First-person body for exploring the Starhouse: walk, run, jump, crouch, lie down and sit.
    ///
    /// Keyboard: WASD move, mouse look, Shift run, Space jump (or get up), C or Ctrl crouch,
    /// Z lie down (Y works too, because Z and Y swap places on German keyboards), X sit,
    /// Esc frees the mouse (click to grab it again).
    /// Gamepad: left stick move, right stick look, L3 run, A jump (or get up), B crouch,
    /// D-pad down lie down, X sit.
    ///
    /// X sits you on a seat-height surface just in front of you (chair, bench, bleacher row, crate, stair):
    /// you turn round and sit on it. With nothing to sit on, you sit on the floor. Any move key gets you up.
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        public Transform cameraPivot;

        [Header("Speeds (m/s)")]
        public float walkSpeed = 2.6f;
        public float sprintSpeed = 4.6f;
        public float crouchSpeed = 1.3f;
        public float crawlSpeed = 0.8f;
        public float jumpHeight = 0.9f;
        public float gravity = -18f;

        [Header("Look")]
        public float mouseSensitivity = 0.12f;
        public float stickSensitivity = 160f;

        [Header("Body (m)")]
        public float standHeight = 1.75f;
        public float crouchHeight = 1.0f;
        [Tooltip("Lying down, the body is a ball as wide as the controller, so it can't be lower than twice the radius.")]
        public float lieHeight = 0.7f;
        public float standEye = 1.63f;
        public float crouchEye = 0.88f;
        public float lieEye = 0.35f;
        public float floorSitEye = 0.8f;
        [Tooltip("Eye height above the seat you are sitting on.")]
        public float seatEye = 0.72f;
        [Tooltip("How quickly the view follows the body when you change stance.")]
        public float eyeSpeed = 10f;

        [Header("Climbing")]
        [Tooltip("Highest step you can walk up (stairs here rise 0.3 m). Under low ceilings it shrinks to the room above you.")]
        public float stepHeight = 0.4f;

        [Header("Sitting")]
        public float seatMinHeight = 0.25f;
        public float seatMaxHeight = 0.95f;
        [Tooltip("How far in front of you a seat can be.")]
        public float seatReach = 1.2f;

        public Stance CurrentStance { get; private set; } = Stance.Standing;

        /// True while sitting on a chair, bench or step rather than on the floor.
        public bool IsOnSeat { get; private set; }

        CharacterController controller;
        float pitch;
        float verticalVelocity;
        float eye;
        Vector3 standUpPosition;
        Stance stanceBeforeSit;
        bool moveHeldAtSit;

        struct Commands
        {
            public Vector2 move;
            public bool run, jump, crouch, lieDown, sit;
        }

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            // Unity's default ignores moves under 1 mm, which freezes the player at high frame rates
            // (crawling at 0.8 m/s stops above ~800 fps), so let every move through.
            controller.minMoveDistance = 0f;
            controller.stepOffset = stepHeight;
            if (cameraPivot == null && Camera.main != null) cameraPivot = Camera.main.transform;
            SetBodyHeight(standHeight);
            eye = standEye;
            PlaceEye();
        }

        void OnEnable() => LockCursor(true);

        static void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            var pad = Gamepad.current;

            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) LockCursor(false);
            if (mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked) LockCursor(true);

            Commands commands = Read(keyboard, pad);
            Look(mouse, pad);
            ChangeStance(ref commands);
            Move(commands);
            FollowEye();
        }

        static Commands Read(Keyboard keyboard, Gamepad pad)
        {
            var c = new Commands();
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed) c.move.y += 1f;
                if (keyboard.sKey.isPressed) c.move.y -= 1f;
                if (keyboard.dKey.isPressed) c.move.x += 1f;
                if (keyboard.aKey.isPressed) c.move.x -= 1f;
                c.run |= keyboard.leftShiftKey.isPressed;
                c.jump |= keyboard.spaceKey.wasPressedThisFrame;
                c.crouch |= keyboard.cKey.wasPressedThisFrame || keyboard.leftCtrlKey.wasPressedThisFrame;
                c.lieDown |= keyboard.zKey.wasPressedThisFrame || keyboard.yKey.wasPressedThisFrame;
                c.sit |= keyboard.xKey.wasPressedThisFrame;
            }
            if (pad != null)
            {
                c.move += pad.leftStick.ReadValue();
                c.run |= pad.leftStickButton.isPressed;
                c.jump |= pad.buttonSouth.wasPressedThisFrame;
                c.crouch |= pad.buttonEast.wasPressedThisFrame;
                c.lieDown |= pad.dpad.down.wasPressedThisFrame;
                c.sit |= pad.buttonWest.wasPressedThisFrame;
            }
            c.move = Vector2.ClampMagnitude(c.move, 1f);
            return c;
        }

        void Look(Mouse mouse, Gamepad pad)
        {
            Vector2 look = Vector2.zero;
            if (mouse != null && Cursor.lockState == CursorLockMode.Locked) look += mouse.delta.ReadValue() * mouseSensitivity;
            if (pad != null) look += pad.rightStick.ReadValue() * stickSensitivity * Time.deltaTime;

            transform.Rotate(0f, look.x, 0f);
            pitch = Mathf.Clamp(pitch - look.y, -85f, 85f);
            if (cameraPivot != null) cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        // ---------------------------------------------------------------- stances

        void ChangeStance(ref Commands c)
        {
            bool moving = c.move.sqrMagnitude > 0.04f;
            if (CurrentStance == Stance.Sitting)
            {
                // A stance button, or starting to walk, gets you up. A move key that was already held
                // when you sat down doesn't count until you let go of it.
                if (!moving) moveHeldAtSit = false;
                bool getUp = c.sit || c.jump || c.crouch || c.lieDown || (moving && !moveHeldAtSit);
                if (!getUp) return;
                GetUpFromSitting();
                if (c.crouch) TrySetStance(Stance.Crouching);
                else if (c.lieDown) TrySetStance(Stance.LyingDown);
                c.jump = false;
                return;
            }

            if (c.sit)
            {
                TrySit();
                if (CurrentStance == Stance.Sitting) moveHeldAtSit = moving;
            }
            else if (c.lieDown)
            {
                if (CurrentStance == Stance.LyingDown) GetUp();
                else if (controller.isGrounded) TrySetStance(Stance.LyingDown);
            }
            else if (c.crouch) TrySetStance(CurrentStance == Stance.Crouching ? Stance.Standing : Stance.Crouching);
            else if (c.jump && CurrentStance != Stance.Standing)
            {
                GetUp();
                c.jump = false; // the press was used to get up, not to jump
            }
        }

        bool TrySetStance(Stance stance)
        {
            float height = BodyHeight(stance);
            if (height > controller.height + 0.01f && !HasHeadroom(height)) return false;
            CurrentStance = stance;
            SetBodyHeight(height);
            return true;
        }

        /// Stand up if there is room, otherwise at least get from lying to crouching.
        void GetUp()
        {
            if (!TrySetStance(Stance.Standing) && CurrentStance == Stance.LyingDown)
                TrySetStance(Stance.Crouching);
        }

        float BodyHeight(Stance stance)
        {
            switch (stance)
            {
                case Stance.Standing: return standHeight;
                case Stance.LyingDown: return Mathf.Max(lieHeight, controller.radius * 2f);
                default: return crouchHeight; // crouching, and sitting on the floor
            }
        }

        /// Is there room above the body to grow to this height? Sweeps the body's own top ball straight up
        /// (so its own collider is ignored), as far as the new top plus the skin the controller keeps from ceilings.
        bool HasHeadroom(float height)
        {
            float extra = height - controller.height;
            if (extra <= 0f) return true;
            Vector3 top = transform.position + Vector3.up * (controller.height - controller.radius);
            return !Physics.SphereCast(top, controller.radius, Vector3.up, out _, extra + controller.skinWidth, ~0, QueryTriggerInteraction.Ignore);
        }

        void SetBodyHeight(float height)
        {
            controller.height = height;
            controller.center = new Vector3(0f, height * 0.5f, 0f);
        }

        // ---------------------------------------------------------------- sitting

        void TrySit()
        {
            if (!controller.isGrounded) return;
            if (FindSeat(out Vector3 seat))
            {
                stanceBeforeSit = CurrentStance;
                standUpPosition = transform.position;
                eye -= seat.y - transform.position.y; // keep the view where it is; it then eases down
                controller.enabled = false;           // parked on the seat: no falling, no colliding
                transform.position = seat;
                transform.Rotate(0f, 180f, 0f);       // turn round and sit, like on a real chair
                verticalVelocity = 0f;
                IsOnSeat = true;
                CurrentStance = Stance.Sitting;
                return;
            }

            // nothing to sit on: sit on the floor right here, with the crouching body
            if (!TrySetStance(Stance.Crouching)) return;
            IsOnSeat = false;
            CurrentStance = Stance.Sitting;
        }

        void GetUpFromSitting()
        {
            if (IsOnSeat)
            {
                eye -= standUpPosition.y - transform.position.y;
                transform.position = standUpPosition;
                controller.enabled = true;
                IsOnSeat = false;
                CurrentStance = stanceBeforeSit; // the body still has the height it had then
            }
            else
            {
                CurrentStance = Stance.Crouching; // floor-sitting uses the crouching body
            }
            GetUp();
        }

        /// Looks for a flat surface at seat height just in front of you, with room to sit on it.
        /// Returns where your hips go.
        bool FindSeat(out Vector3 hips)
        {
            hips = default;
            Vector3 feet = transform.position;
            feet.y = GroundHeight(); // the controller floats a little above the ground, by up to its skin width
            Vector3 forward = transform.forward;
            float probeTop = seatMaxHeight + 0.05f;

            for (float d = controller.radius + 0.1f; d <= seatReach; d += 0.1f)
            {
                Vector3 probe = feet + forward * d + Vector3.up * probeTop;
                if (!Physics.Raycast(probe, Vector3.down, out RaycastHit hit, probeTop + 0.2f, ~0, QueryTriggerInteraction.Ignore))
                    continue; // a gap: look further ahead
                float height = hit.point.y - feet.y;
                if (height < seatMinHeight) continue; // still floor: look further ahead
                if (height > seatMaxHeight || hit.normal.y < 0.8f) return false;

                // nothing in the way between you and the seat's front edge
                Vector3 low = feet + Vector3.up * (height + 0.1f);
                if (LineBlocked(low, low + forward * d)) return false;

                // the hips go a little way onto the seat, which must still be there and level
                Vector3 seat = hit.point + forward * 0.15f;
                if (!Physics.Raycast(seat + Vector3.up * 0.3f, Vector3.down, out RaycastHit seatHit, 0.45f, ~0, QueryTriggerInteraction.Ignore)
                    || Mathf.Abs(seatHit.point.y - hit.point.y) > 0.08f)
                    return false;
                seat.y = seatHit.point.y;

                // there must be room above it for a sitting body (starting high enough to clear a stair step behind the hips)
                if (Blocked(seat + Vector3.up * 0.5f, seat + Vector3.up * (seatEye + 0.1f), 0.18f)) return false;

                // and nothing between your head and where it will be (no sitting up through a desk you're lying under)
                if (LineBlocked(transform.position + Vector3.up * TargetEye(), seat + Vector3.up * seatEye)) return false;

                hips = seat;
                return true;
            }
            return false;
        }

        bool Blocked(Vector3 bottom, Vector3 top, float radius)
        {
            foreach (var other in Physics.OverlapCapsule(bottom, top, radius, ~0, QueryTriggerInteraction.Ignore))
                if (other != controller) return true;
            return false;
        }

        /// Is anything but our own body on the line from a to b? (The controller's own collider can't be trusted
        /// to be skipped just because a line starts inside the body: at eye height it can count as outside.)
        bool LineBlocked(Vector3 a, Vector3 b)
        {
            Vector3 along = b - a;
            float length = along.magnitude;
            if (length < 0.001f) return false;
            foreach (var hit in Physics.RaycastAll(a, along / length, length, ~0, QueryTriggerInteraction.Ignore))
                if (hit.collider != controller) return true;
            return false;
        }

        /// Height of the ground right under the body.
        float GroundHeight()
        {
            float ground = float.NegativeInfinity;
            foreach (var hit in Physics.RaycastAll(transform.position + Vector3.up * 0.3f, Vector3.down, 0.6f, ~0, QueryTriggerInteraction.Ignore))
                if (hit.collider != controller && hit.point.y > ground) ground = hit.point.y;
            return ground > float.NegativeInfinity ? ground : transform.position.y - controller.skinWidth;
        }

        // ---------------------------------------------------------------- moving and the view

        void Move(in Commands c)
        {
            if (IsOnSeat) return; // parked on a seat

            float speed;
            switch (CurrentStance)
            {
                case Stance.Crouching: speed = crouchSpeed; break;
                case Stance.LyingDown: speed = crawlSpeed; break;
                case Stance.Sitting: speed = 0f; break;
                default: speed = c.run ? sprintSpeed : walkSpeed; break;
            }
            Vector3 move = (transform.right * c.move.x + transform.forward * c.move.y) * speed;

            if (controller.isGrounded)
            {
                verticalVelocity = -2f;
                if (c.jump && CurrentStance == Stance.Standing) verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            verticalVelocity += gravity * Time.deltaTime;
            move.y = verticalVelocity;
            LimitStepToHeadroom();
            controller.Move(move * Time.deltaTime);
        }

        /// The controller climbs steps by lifting the body by stepOffset before moving sideways. Under a low ceiling
        /// that lift jams the body against the ceiling's edge, so you couldn't crawl or crouch into low gaps.
        /// So the lift is limited to the room above the body, and a little around it, where it is heading.
        void LimitStepToHeadroom()
        {
            float skin = controller.skinWidth;
            float reach = controller.radius + skin + 0.1f;
            Vector3 start = transform.position + Vector3.up * (controller.height + 0.01f);
            float room = stepHeight + skin + 0.02f;
            if (Physics.BoxCast(start, new Vector3(reach, 0.01f, reach), Vector3.up, out RaycastHit hit, transform.rotation, room, ~0, QueryTriggerInteraction.Ignore))
                room = hit.distance;
            controller.stepOffset = Mathf.Clamp(room - skin - 0.02f, 0f, stepHeight);
        }

        float TargetEye()
        {
            switch (CurrentStance)
            {
                case Stance.Standing: return standEye;
                case Stance.Crouching: return crouchEye;
                case Stance.LyingDown: return lieEye;
                default: return IsOnSeat ? seatEye : floorSitEye;
            }
        }

        void FollowEye()
        {
            eye = Mathf.Lerp(eye, TargetEye(), 1f - Mathf.Exp(-eyeSpeed * Time.deltaTime));
            PlaceEye();
        }

        void PlaceEye()
        {
            if (cameraPivot != null) cameraPivot.localPosition = new Vector3(0f, eye, 0f);
        }
    }
}
