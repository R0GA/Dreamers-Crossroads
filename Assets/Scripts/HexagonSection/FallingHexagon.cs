using UnityEngine;
using System.Collections;

public class FallingHexagon : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Tooltip("The NON-trigger collider the player stands on.")]
    [SerializeField] private Collider solidCollider;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("If true, stepping on this hexagon triggers it to fall. " +
             "Turn this OFF when using random falling.")]
    [SerializeField] private bool fallOnPlayerStep = false;

    [Header("Warning")]
    [Tooltip("How long the tile warns the player before falling.")]
    [SerializeField] private float warningDuration = 0.8f;

    [Tooltip("How strongly the tile shakes during the warning.")]
    [SerializeField] private float shakeAmount = 0.04f;

    [Tooltip("How quickly the tile shakes.")]
    [SerializeField] private float shakeSpeed = 30f;

    [Tooltip("Colour the tile flashes toward before falling.")]
    [SerializeField] private Color warningColor = Color.white;

    [Header("Fall")]
    [SerializeField] private float fallSpeed = 8f;
    [Tooltip("How long the tile falls before being hidden.")]
    [SerializeField] private float fallDuration = 1.5f;

    //[SerializeField] private bool deactivateAfterFall = true;

    [Header("Return")]
    [Tooltip("How long the hexagon stays gone before returning.")]
    [SerializeField] private float returnDelay = 3f;

    private Vector3 startingLocalPosition;
    private Quaternion startingLocalRotation;
    private Color startingColor;

    private bool triggered;
    private Coroutine fallRoutine;

    public bool HasTriggered => triggered;

    private void Awake()
    {
        startingLocalPosition = transform.localPosition;
        startingLocalRotation = transform.localRotation;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            startingColor = spriteRenderer.color;

        if (solidCollider == null)
        {
            Collider[] colliders = GetComponents<Collider>();

            foreach (Collider col in colliders)
            {
                if (!col.isTrigger)
                {
                    solidCollider = col;
                    break;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Random falling is controlling these tiles for now, off under grapple shtuff for safety        if (!fallOnPlayerStep)
            return;

        if (!other.CompareTag(playerTag))
            return;

        TriggerFall();
    }

    public void TriggerFall()
    {
        if (triggered)
            return;

        triggered = true;

        if (fallRoutine != null)
            StopCoroutine(fallRoutine);

        fallRoutine = StartCoroutine(FallRoutine());
    }

    private IEnumerator FallRoutine()
    {
        float timer = 0f;

        while (timer < warningDuration)
        {
            timer += Time.deltaTime;

            float shakeX = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
            float shakeZ = Mathf.Cos(Time.time * shakeSpeed * 0.8f) * shakeAmount;

            transform.localPosition =
                startingLocalPosition +
                new Vector3(shakeX, 0f, shakeZ);

            if (spriteRenderer != null)
            {
                float pulse = Mathf.PingPong(timer * 5f, 1f);

                spriteRenderer.color =
                    Color.Lerp(startingColor, warningColor, pulse);
            }

            yield return null;
        }

        transform.localPosition = startingLocalPosition;

        if (spriteRenderer != null)
            spriteRenderer.color = startingColor;

        // Player can no longer stand on it, double checck
        if (solidCollider != null)
            solidCollider.enabled = false;


        timer = 0f;

        while (timer < fallDuration)
        {
            timer += Time.deltaTime;

            transform.position +=
                Vector3.down * fallSpeed * Time.deltaTime;

            yield return null;
        }

        //if (deactivateAfterFall)
        //    gameObject.SetActive(false);

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        yield return new WaitForSeconds(returnDelay);

        transform.localPosition = startingLocalPosition;
        transform.localRotation = startingLocalRotation;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = startingColor;
            spriteRenderer.enabled = true;
        }


        if (solidCollider != null)
            solidCollider.enabled = true;

        triggered = false;
        fallRoutine = null;
    }

    // Use later when resetting the section after death? Also, ermmmm make so that all hexagons only fall when getting to this section
    public void ResetHexagon()
    {
        if (fallRoutine != null)
            StopCoroutine(fallRoutine);

        //gameObject.SetActive(true);

        triggered = false;

        transform.localPosition = startingLocalPosition;
        transform.localRotation = startingLocalRotation;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = startingColor;
            spriteRenderer.enabled = true;
        }

        if (solidCollider != null)
            solidCollider.enabled = true;

        fallRoutine = null;
    }
}
