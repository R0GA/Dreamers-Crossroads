using UnityEngine;
using UnityEngine.Events;

namespace PuzzlePlatformer.Puzzles
{
    /// <summary>
    /// Owns a set of RingPuzzleRing objects, routes rotation requests to them,
    /// and fires onPuzzleSolved once every ring shows its correct symbol.
    /// Put this on the puzzle door's root GameObject.
    /// </summary>
    public class RingPuzzleController : MonoBehaviour
    {
        [Header("Rings (order doesn't matter for solving)")]
        public RingPuzzleRing[] rings;

        [Header("Events")]
        public UnityEvent onPuzzleSolved;
        public UnityEvent onRingRotated;

        public bool IsSolved { get; private set; }

        /// <summary>Called by the interactor when the player tries to spin a ring.</summary>
        public void TryRotateRing(RingPuzzleRing ring, int direction)
        {
            if (IsSolved || ring == null) return;

            ring.Rotate(direction);
            onRingRotated?.Invoke();
            CheckSolution();
        }

        void CheckSolution()
        {
            if (IsSolved) return;

            foreach (var ring in rings)
            {
                if (ring == null || !ring.IsCorrect) return;
            }

            IsSolved = true;
            onPuzzleSolved?.Invoke();
        }

        /// <summary>Optional: call from a reset trigger/lever to let players retry after a fail state.</summary>
        public void ResetPuzzle()
        {
            IsSolved = false;
        }
    }
}
