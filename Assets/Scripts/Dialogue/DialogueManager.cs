using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central hub for playing narrative dialogue. Anything can request a DialogueSequence be
/// played — trigger zones, interactables, quest/cutscene logic — without knowing anything
/// about UI or audio. DialogueUI (or any other listener) reacts to the events fired here to
/// actually display text and play voice clips.
///
/// Persistent singleton: survives scene loads so dialogue state (and the "already played"
/// set for one-shot sequences) isn't lost between levels. Add one DialogueManager to your
/// first scene and forget about it.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Audio")]
    [Tooltip("Used to play each line's voiceClip, if it has one. Auto-filled from this "
           + "GameObject's AudioSource if left empty.")]
    [SerializeField] private AudioSource voiceSource;

    // ── Public State ─────────────────────────────────────────────────────────

    public bool IsPlaying => currentSequence != null;
    public DialogueSequence CurrentSequence => currentSequence;

    public DialogueLine CurrentLine =>
        IsPlaying && currentLineIndex < currentSequence.lines.Length
            ? currentSequence.lines[currentLineIndex]
            : null;

    // ── Events ───────────────────────────────────────────────────────────────

    /// <summary>Fired once when a sequence begins playing.</summary>
    public event Action<DialogueSequence> DialogueStarted;

    /// <summary>Fired every time a new line becomes current (including the first).</summary>
    public event Action<DialogueLine> LineStarted;

    /// <summary>Fired once when a sequence finishes, naturally or via Stop().</summary>
    public event Action<DialogueSequence> DialogueEnded;

    // ── Private State ────────────────────────────────────────────────────────

    private DialogueSequence currentSequence;
    private int currentLineIndex;
    private Coroutine playRoutine;
    private bool advanceRequested;
    private readonly HashSet<DialogueSequence> playedOnce = new HashSet<DialogueSequence>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (voiceSource == null)
            voiceSource = GetComponent<AudioSource>();
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Requests that a sequence play. Returns false (and does nothing) if dialogue is
    /// already in progress, or if the sequence is marked playOnce and has already played.
    /// Callers that care whether the request went through (e.g. to only mark a trigger zone
    /// as "fired" on success) should check the return value.
    /// </summary>
    public bool PlayDialogue(DialogueSequence sequence)
    {
        if (sequence == null || sequence.lines == null || sequence.lines.Length == 0)
            return false;

        if (IsPlaying) return false;
        if (sequence.playOnce && playedOnce.Contains(sequence)) return false;

        playRoutine = StartCoroutine(PlaySequenceRoutine(sequence));
        return true;
    }

    /// <summary>
    /// Advances past the current line early. Call this from UI input handling (e.g. pressing
    /// Interact/Continue while dialogue is on screen). No effect if nothing is playing.
    /// </summary>
    public void Advance() => advanceRequested = true;

    /// <summary>Immediately halts whatever sequence is playing, firing DialogueEnded.</summary>
    public void Stop()
    {
        if (!IsPlaying) return;

        if (playRoutine != null) StopCoroutine(playRoutine);
        voiceSource.Stop();

        DialogueSequence ended = currentSequence;
        currentSequence = null;
        currentLineIndex = 0;
        DialogueEnded?.Invoke(ended);
    }

    // ── Playback ─────────────────────────────────────────────────────────────

    private IEnumerator PlaySequenceRoutine(DialogueSequence sequence)
    {
        currentSequence = sequence;
        currentLineIndex = 0;
        DialogueStarted?.Invoke(sequence);

        for (int i = 0; i < sequence.lines.Length; i++)
        {
            currentLineIndex = i;
            DialogueLine line = sequence.lines[i];
            LineStarted?.Invoke(line);

            if (line.voiceClip != null)
                voiceSource.PlayOneShot(line.voiceClip);

            advanceRequested = false;

            if (line.autoAdvanceDelay > 0f)
                yield return new WaitForSeconds(line.autoAdvanceDelay);
            else
                yield return new WaitUntil(() => advanceRequested); // DialogueUI calls Advance()
        }

        if (sequence.playOnce)
            playedOnce.Add(sequence);

        currentSequence = null;
        currentLineIndex = 0;
        DialogueEnded?.Invoke(sequence);
    }
}
