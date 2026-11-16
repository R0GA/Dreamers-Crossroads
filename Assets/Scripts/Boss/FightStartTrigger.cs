using UnityEngine;

/// <summary>Trigger volume at the arena entrance. Starts the fight the first time the player walks in.</summary>
[RequireComponent(typeof(Collider))]
public class FightStartTrigger : MonoBehaviour
{
    [SerializeField] private SirNightmareFight fight;
    [SerializeField] private AudioSource levelAudio;
    [SerializeField] private AudioClip bossMusic;

    private bool used;

    private void Reset() => GetComponent<Collider>().isTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (used || fight == null) return;
        if (other.GetComponentInParent<PlayerController>() == null) return;

        used = true;
        fight.StartFight();
        levelAudio.clip = bossMusic;
        levelAudio.Play();
    }
}
