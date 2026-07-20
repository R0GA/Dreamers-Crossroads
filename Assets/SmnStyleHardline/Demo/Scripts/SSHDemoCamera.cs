namespace SmnStyleHardline.Demo
{
    /*
 * SSH Demo Camera
 * Runtime demo camera supporting free-fly navigation and
 * cinematic preset transitions for presentation and testing.
 */

    using UnityEngine;

#if ENABLE_INPUT_SYSTEM
    using UnityEngine.InputSystem;
#endif


    // ==========================================================================
    // Demo Camera
    // ==========================================================================

    public enum SSHDemoCameraMode
    {
        Freecam,
        Cinematic
    }

    public class SSHDemoCamera : MonoBehaviour
    {
        // ======================================================================
        // Mode
        // ======================================================================

        [Header("Mode")]
        public SSHDemoCameraMode mode = SSHDemoCameraMode.Freecam;


        // ======================================================================
        // Freecam Settings
        // ======================================================================

        [Header("Freecam Settings")]
        public float moveSpeed = 5f;
        public float boostMultiplier = 3f;
        public float lookSensitivity = 2f;

        public float scrollSpeedStep = 1f;
        public float minMoveSpeed = 0.1f;
        public float maxMoveSpeed = 200f;


        // ======================================================================
        // Cinematic Settings
        // ======================================================================

        [Header("Cinematic Settings")]
        public Transform[] cinematicPresets;
        public float cinematicMoveDuration = 2.5f;
        public AnimationCurve cinematicEase =
            AnimationCurve.EaseInOut(0, 0, 1, 1);


        // ======================================================================
        // Editor Testing
        // ======================================================================

        [Header("Editor Testing")]
        public Transform currentCinematicTarget;


        // ======================================================================
        // State
        // ======================================================================

        float yaw;
        float pitch;

        Coroutine cinematicRoutine;
        Transform lastCinematicTarget;
        int currentCinematicIndex;


        // ======================================================================
        // Unity Lifecycle
        // ======================================================================

        void Start()
        {
            Vector3 rot = transform.eulerAngles;
            yaw = rot.y;
            pitch = rot.x;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            lastCinematicTarget = currentCinematicTarget;
            currentCinematicIndex = FindInitialCinematicIndex();
        }

        void Update()
        {
            switch (mode)
            {
                case SSHDemoCameraMode.Freecam:
                    UpdateFreecam();
                    break;

                case SSHDemoCameraMode.Cinematic:
                    UpdateCinematicEditorTesting();
                    UpdateCinematicInput();
                    break;
            }
        }


        // ======================================================================
        // Freecam
        // ======================================================================

        void UpdateFreecam()
        {
            bool rmbHeld = IsRightMouseHeld();

            Cursor.lockState =
                rmbHeld ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !rmbHeld;

            if (!rmbHeld)
                return;

            float scroll = GetScrollDelta();
            if (Mathf.Abs(scroll) > 0.01f)
            {
                moveSpeed = Mathf.Clamp(
                    moveSpeed + scroll * scrollSpeedStep,
                    minMoveSpeed,
                    maxMoveSpeed
                );
            }

            Vector2 look = GetMouseDelta();
            yaw += look.x * lookSensitivity;
            pitch -= look.y * lookSensitivity;
            pitch = Mathf.Clamp(pitch, -89f, 89f);

            transform.rotation =
                Quaternion.Euler(pitch, yaw, 0f);

            float speed = moveSpeed;
            if (IsBoostHeld())
                speed *= boostMultiplier;

            Vector3 dir = Vector3.zero;

            if (IsKeyHeld(KeyCode.W)) dir += transform.forward;
            if (IsKeyHeld(KeyCode.S)) dir -= transform.forward;
            if (IsKeyHeld(KeyCode.A)) dir -= transform.right;
            if (IsKeyHeld(KeyCode.D)) dir += transform.right;
            if (IsKeyHeld(KeyCode.Q)) dir -= transform.up;
            if (IsKeyHeld(KeyCode.E)) dir += transform.up;

            transform.position += dir * speed * Time.deltaTime;
        }


        // ======================================================================
        // Cinematic
        // ======================================================================

        void UpdateCinematicEditorTesting()
        {
            if (!Application.isPlaying)
                return;

            if (currentCinematicTarget != lastCinematicTarget)
            {
                lastCinematicTarget = currentCinematicTarget;

                if (currentCinematicTarget != null)
                    StartCinematicMove(currentCinematicTarget);
            }
        }

        void UpdateCinematicInput()
        {
            if (!Application.isPlaying)
                return;

            if (IsKeyPressed(KeyCode.A))
                StepCinematic(-1);

            if (IsKeyPressed(KeyCode.D))
                StepCinematic(1);
        }

        public void EnterCinematicMode()
        {
            mode = SSHDemoCameraMode.Cinematic;
        }

        public void EnterFreecamMode()
        {
            if (cinematicRoutine != null)
            {
                StopCoroutine(cinematicRoutine);
                cinematicRoutine = null;
            }

            Vector3 rot = transform.eulerAngles;
            yaw = rot.y;
            pitch = rot.x;

            mode = SSHDemoCameraMode.Freecam;
        }

        public void MoveToCinematicPreset(int index)
        {
            if (cinematicPresets == null ||
                cinematicPresets.Length == 0)
                return;

            if (index < 0 ||
                index >= cinematicPresets.Length)
                return;

            currentCinematicIndex = index;
            StartCinematicMove(cinematicPresets[index]);
        }

        void StepCinematic(int direction)
        {
            if (cinematicPresets == null ||
                cinematicPresets.Length == 0)
                return;

            currentCinematicIndex =
                (currentCinematicIndex + direction +
                 cinematicPresets.Length)
                % cinematicPresets.Length;

            MoveToCinematicPreset(currentCinematicIndex);
        }

        int FindInitialCinematicIndex()
        {
            if (cinematicPresets == null ||
                currentCinematicTarget == null)
                return 0;

            for (int i = 0; i < cinematicPresets.Length; i++)
            {
                if (cinematicPresets[i] == currentCinematicTarget)
                    return i;
            }

            return 0;
        }

        void StartCinematicMove(Transform target)
        {
            if (cinematicRoutine != null)
                StopCoroutine(cinematicRoutine);

            cinematicRoutine =
                StartCoroutine(CinematicMove(target));
        }

        System.Collections.IEnumerator CinematicMove(Transform target)
        {
            mode = SSHDemoCameraMode.Cinematic;

            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;

            Vector3 endPos = target.position;
            Quaternion endRot = target.rotation;

            float elapsed = 0f;

            while (elapsed < cinematicMoveDuration)
            {
                elapsed += Time.deltaTime;

                float t =
                    Mathf.Clamp01(elapsed / cinematicMoveDuration);
                float eased =
                    cinematicEase.Evaluate(t);

                transform.position =
                    Vector3.Lerp(startPos, endPos, eased);
                transform.rotation =
                    Quaternion.Slerp(startRot, endRot, eased);

                yield return new WaitForEndOfFrame();
            }

            transform.position = endPos;
            transform.rotation = endRot;

            cinematicRoutine = null;
        }


        // ======================================================================
        // Input Abstraction
        // ======================================================================

        float GetScrollDelta()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current == null) return 0f;
            return Mouse.current.scroll.ReadValue().y * 0.01f;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.mouseScrollDelta.y;
#else
        return 0f;
#endif
        }

        Vector2 GetMouseDelta()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null
                ? Mouse.current.delta.ReadValue()
                : Vector2.zero;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y"));
#else
        return Vector2.zero;
#endif
        }

        bool IsRightMouseHeld()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null &&
                   Mouse.current.rightButton.isPressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetMouseButton(1);
#else
        return false;
#endif
        }

        bool IsBoostHeld()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null &&
                   Keyboard.current.leftShiftKey.isPressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKey(KeyCode.LeftShift);
#else
        return false;
#endif
        }

        bool IsKeyHeld(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current == null)
                return false;

            return key switch
            {
                KeyCode.W => Keyboard.current.wKey.isPressed,
                KeyCode.A => Keyboard.current.aKey.isPressed,
                KeyCode.S => Keyboard.current.sKey.isPressed,
                KeyCode.D => Keyboard.current.dKey.isPressed,
                KeyCode.Q => Keyboard.current.qKey.isPressed,
                KeyCode.E => Keyboard.current.eKey.isPressed,
                _ => false
            };
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKey(key);
#else
        return false;
#endif
        }

        bool IsKeyPressed(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current == null)
                return false;

            return key switch
            {
                KeyCode.A => Keyboard.current.aKey.wasPressedThisFrame,
                KeyCode.D => Keyboard.current.dKey.wasPressedThisFrame,
                _ => false
            };
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(key);
#else
        return false;
#endif
        }
    }

}