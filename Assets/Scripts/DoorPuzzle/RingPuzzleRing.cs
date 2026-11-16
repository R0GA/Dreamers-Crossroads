using UnityEngine;

namespace PuzzlePlatformer.Puzzles
{
    /// <summary>
    /// Represents one rotating ring in a Bleak-Falls-Barrow-style symbol door.
    /// Attach this to the PIVOT object that the ring's visual mesh is parented under
    /// (the pivot's origin should sit on the ring's rotation axis).
    /// </summary>
    public class RingPuzzleRing : MonoBehaviour, IInteractable
    {
        public enum Axis { X, Y, Z }

        [Header("Ring Configuration")]
        [Tooltip("How many symbols are evenly spaced around this ring.")]
        [Min(2)] public int symbolCount = 6;

        [Tooltip("Index (0-based) of the symbol that must land on the alignment marker to solve this ring.")]
        public int correctIndex = 0;

        [Tooltip("Local axis the ring spins around. Usually Z if the door faces the player down the Z axis.")]
        public Axis rotationAxis = Axis.Z;

        [Header("Rotation Feel")]
        [Tooltip("Degrees per second while easing into the next symbol slot.")]
        public float rotationSpeed = 180f;

        [Tooltip("Flip if a 'forward' click spins the wrong way visually.")]
        public bool invertDirection = false;

        [Header("Audio (optional)")]
        public AudioSource audioSource;
        public AudioClip stepSound;

        /// <summary>Current symbol index resting (or settling) at the alignment marker.</summary>
        public int CurrentIndex { get; private set; }

        /// <summary>True once this ring's current symbol matches correctIndex.</summary>
        public bool IsCorrect => CurrentIndex == correctIndex;

        /// <summary>True while the ring is still easing toward its target rotation.</summary>
        public bool IsRotating => Quaternion.Angle(transform.localRotation, _targetLocalRotation) > 0.05f;

        int _totalSteps;
        Quaternion _baseLocalRotation;
        Quaternion _targetLocalRotation;
        RingPuzzleController _controller;

        float DegreesPerSymbol => 360f / symbolCount;

        Vector3 AxisVector
        {
            get
            {
                switch (rotationAxis)
                {
                    case Axis.X: return Vector3.right;
                    case Axis.Y: return Vector3.up;
                    default: return Vector3.forward;
                }
            }
        }

        void Awake()
        {
            _baseLocalRotation = transform.localRotation;
            _targetLocalRotation = _baseLocalRotation;
            _controller = GetComponentInParent<RingPuzzleController>();

            if (_controller == null)
                Debug.LogWarning($"{name}: no RingPuzzleController found in parents.", this);
        }

        // ── IInteractable ─────────────────────────────────────────────────────
        // Only this ring rotates: the player's raycast hits THIS ring's own collider,
        // so Interact() is called on this specific instance and no other ring is touched.

        public void Interact(GameObject interactor) => _controller?.TryRotateRing(this, 1);
        public void InteractAlt(GameObject interactor) => _controller?.TryRotateRing(this, -1);
        public string GetInteractionPrompt() => "Rotate Ring";

        void Update()
        {
            if (IsRotating)
            {
                transform.localRotation = Quaternion.RotateTowards(
                    transform.localRotation, _targetLocalRotation, rotationSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Rotate the ring by exactly one symbol slot. Pass +1 or -1.
        /// Can be called again mid-animation; the new target just queues smoothly.
        /// </summary>
        public void Rotate(int direction)
        {
            if (direction == 0) return;
            direction = direction > 0 ? 1 : -1;
            if (invertDirection) direction *= -1;

            _totalSteps += direction;
            CurrentIndex = ((_totalSteps % symbolCount) + symbolCount) % symbolCount;
            _targetLocalRotation = _baseLocalRotation * Quaternion.AngleAxis(_totalSteps * DegreesPerSymbol, AxisVector);

            if (stepSound && audioSource) audioSource.PlayOneShot(stepSound);
        }

#if UNITY_EDITOR
        // Lets you preview correctIndex in the editor by right-clicking the component -> "Snap To Correct Index (Preview)"
        [ContextMenu("Snap To Correct Index (Preview)")]
        void PreviewCorrect()
        {
            _baseLocalRotation = transform.localRotation;
            _totalSteps = correctIndex;
            CurrentIndex = correctIndex;
            _targetLocalRotation = _baseLocalRotation * Quaternion.AngleAxis(_totalSteps * DegreesPerSymbol, AxisVector);
            transform.localRotation = _targetLocalRotation;
        }
#endif
    }
}
