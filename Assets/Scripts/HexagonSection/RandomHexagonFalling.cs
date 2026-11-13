using System.Collections.Generic;
using UnityEngine;
using System.Collections;


public class RandomHexagonFalling : MonoBehaviour
{
    [Header("Hexagons")]
    [Tooltip("If left empty, automatically finds all FallingHexagon scripts under this object.")]
    [SerializeField] private FallingHexagon[] hexagons;

    [Header("Random Fall Timing")]
    [Tooltip("Delay before random falling begins.")]
    [SerializeField] private float startDelay = 2f;

    [Tooltip("Shortest possible time between random falls.")]
    [SerializeField] private float minTimeBetweenFalls = 1.5f;

    [Tooltip("Longest possible time between random falls.")]
    [SerializeField] private float maxTimeBetweenFalls = 3f;

    [Header("Amount")]
    [Tooltip("How many random hexagons should begin falling at each interval.")]
    [SerializeField] private int tilesPerWave = 1;

    [Header("Behaviour")]
    [Tooltip("Automatically start random falling when this object becomes active.")]
    [SerializeField] private bool autoStart = true;

    private Coroutine randomFallRoutine;
    private bool running;

    private void Awake()
    {
        // Automatically be collecting them FallingHexagon components under the Middle Hard parent
        if (hexagons == null || hexagons.Length == 0)
            hexagons = GetComponentsInChildren<FallingHexagon>(true);
    }

    private void Start()
    {
        if (autoStart)
            StartRandomFalls();
    }

    public void StartRandomFalls()
    {
        if (running)
            return;

        running = true;
        randomFallRoutine = StartCoroutine(RandomFallRoutine());
    }

    public void StopRandomFalls()
    {
        running = false;

        if (randomFallRoutine != null)
        {
            StopCoroutine(randomFallRoutine);
            randomFallRoutine = null;
        }
    }

    private IEnumerator RandomFallRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        while (running)
        {
            TriggerRandomTiles();

            float waitTime =
                Random.Range(minTimeBetweenFalls, maxTimeBetweenFalls);

            yield return new WaitForSeconds(waitTime);
        }
    }

    private void TriggerRandomTiles()
    {
        List<FallingHexagon> availableTiles =
            new List<FallingHexagon>();

        foreach (FallingHexagon hexagon in hexagons)
        {
            if (hexagon == null)
                continue;

            if (!hexagon.gameObject.activeInHierarchy)
                continue;

            if (hexagon.HasTriggered)
                continue;

            availableTiles.Add(hexagon);
        }

        if (availableTiles.Count == 0)
            return;

        int amount = Mathf.Min(
            tilesPerWave,
            availableTiles.Count
        );

        for (int i = 0; i < amount; i++)
        {
            int randomIndex =
                Random.Range(0, availableTiles.Count);

            FallingHexagon chosen =
                availableTiles[randomIndex];

            availableTiles.RemoveAt(randomIndex);

            chosen.TriggerFall();
        }
    }
}
