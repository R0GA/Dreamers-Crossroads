using UnityEngine;

public class PlaySoundOnce : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioSource audioSource;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    private bool hasPlayed = false;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasPlayed)
            return;

        if (!other.CompareTag(playerTag))
            return;

        if (audioSource == null)
            return;

        hasPlayed = true;

        audioSource.Play();

        Collider triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null)
            triggerCollider.enabled = false;
    }
}
