using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PuzzlePlatformer.Puzzles
{
    /// <summary>
    /// Hook this up to RingPuzzleController's onPuzzleSolved event in the Inspector.
    /// Keeps the door completely decoupled from puzzle logic.
    /// </summary>
    public class PuzzleDoorController : MonoBehaviour
    {
        public Animator doorAnimator;
        public string openTriggerName = "Open";

        public AudioSource audioSource;
        public AudioClip openSound;

        public void OpenDoor()
        {
           StartCoroutine(OpenDoorRoutine());
        }

        private IEnumerator OpenDoorRoutine()
        {
            yield return new WaitForSeconds(2f); // Optional delay before opening the door
            Debug.Log("Puzzle solved! Opening door...");
            SceneManager.LoadScene("EndSceneTutorial");
            if (doorAnimator != null) doorAnimator.SetTrigger(openTriggerName);
            if (audioSource != null && openSound != null) audioSource.PlayOneShot(openSound);
        }

    }
}
