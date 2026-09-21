using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Birthday cake candle puzzle.
/// Candles must be lit in a hidden order. Lighting a wrong candle resets the whole puzzle.
/// The solution never changes between attempts.
///
/// SOLUTION ORDER:
///  - Default: the order of the 'candles' array IS the solution (Element 0 must be lit first, etc.).
///    The player can't see this order, so arrange them in the inspector however you like.
///  - Optional: tick 'Use Random Solution' to shuffle the order once, using a fixed seed,
///    so it's different per playthrough but constant within it.
/// </summary>
public class CandlePuzzle : MonoBehaviour
{
    [Header("Candles")]
    [Tooltip("Drag every candle here. Array order = solution order (unless Use Random Solution is on).")]
    [SerializeField] private Candle[] candles;

    [Header("Solution")]
    [SerializeField] private bool useRandomSolution = false;
    [Tooltip("Same seed = same order. Set this from your save data / run seed if you want a per-playthrough order.")]
    [SerializeField] private int seed = 12345;

    [Header("Reset Timing")]
    [Tooltip("Pause after a wrong candle is lit, so the player sees their mistake.")]
    [SerializeField] private float wrongPauseSeconds = 0.75f;
    [Tooltip("Delay between each candle blowing out during a reset (0 = all at once).")]
    [SerializeField] private float extinguishStagger = 0.1f;
    [Tooltip("Pause after all candles are out before the player can try again.")]
    [SerializeField] private float afterResetPauseSeconds = 0.5f;

    [Header("Events")]
    public UnityEvent onCorrectCandle;   // each right candle (e.g. play a little chime)
    public UnityEvent onWrongCandle;     // wrong candle (e.g. sad trombone, cake shake)
    public UnityEvent onPuzzleReset;     // once all candles are out again
    public UnityEvent onPuzzleSolved;    // open door, play birthday song, etc.

    private Candle[] solution;
    private int progress;
    private bool busy;

    public bool Solved { get; private set; }
    public bool AcceptingInput => !busy && !Solved;

    private void Awake()
    {
        BuildSolution();
        foreach (var candle in candles)
            candle.Initialise(this);
    }

    private void BuildSolution()
    {
        solution = (Candle[])candles.Clone();

        if (!useRandomSolution) return;

        // Fisher-Yates shuffle with a seeded RNG so the order stays fixed for a given seed.
        var rng = new System.Random(seed);
        for (int i = solution.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (solution[i], solution[j]) = (solution[j], solution[i]);
        }
    }

    /// <summary>Called by a Candle when the player lights it.</summary>
    public void TryLight(Candle candle)
    {
        if (!AcceptingInput || candle.IsLit) return;

        candle.Light();

        if (candle == solution[progress])
        {
            progress++;
            onCorrectCandle?.Invoke();

            if (progress >= solution.Length)
                Solve();
        }
        else
        {
            StartCoroutine(FailRoutine());
        }
    }

    private void Solve()
    {
        Solved = true;
        onPuzzleSolved?.Invoke();
    }

    private IEnumerator FailRoutine()
    {
        busy = true;
        onWrongCandle?.Invoke();

        yield return new WaitForSeconds(wrongPauseSeconds);

        // Blow out candles in the order they were lit, or all at once if stagger is 0.
        foreach (var candle in candles)
        {
            candle.Extinguish();
            if (extinguishStagger > 0f)
                yield return new WaitForSeconds(extinguishStagger);
        }

        yield return new WaitForSeconds(afterResetPauseSeconds);

        progress = 0;
        busy = false;
        onPuzzleReset?.Invoke();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (candles == null) return;

        var seen = new HashSet<Candle>();
        foreach (var c in candles)
        {
            if (c == null) { Debug.LogWarning("CandlePuzzle: a candle slot is empty.", this); continue; }
            if (!seen.Add(c)) Debug.LogWarning($"CandlePuzzle: '{c.name}' is in the list twice.", this);
        }
    }
#endif
}
