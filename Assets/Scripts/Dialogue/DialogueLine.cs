using UnityEngine;

/// <summary>
/// A single beat of dialogue: who's speaking, what they say, and optionally a voice clip
/// and portrait. Lives inside a DialogueSequence — not a standalone asset.
/// </summary>
[System.Serializable]
public class DialogueLine
{
    [Tooltip("Shown in the speaker name field. Leave blank for unattributed/internal lines.")]
    public string speakerName;

    [Tooltip("The line itself.")]
    [TextArea(2, 5)]
    public string text;

    [Tooltip("Optional voice-over clip played when this line starts.")]
    public AudioClip voiceClip;

    [Tooltip("Optional portrait shown alongside this line.")]
    public Sprite portrait;

    [Tooltip("If greater than 0, this line auto-advances after this many seconds instead of "
           + "waiting for player input. Leave at 0 for player-paced dialogue.")]
    public float autoAdvanceDelay = 0f;
}
