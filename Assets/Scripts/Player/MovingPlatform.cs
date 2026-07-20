using UnityEngine;

/// <summary>
/// Marker + mover for a moving platform. PlayerController looks for this component
/// (via GetComponentInParent, so it's fine if the collider lives on a child mesh)
/// to detect what it's standing on and ride along with it.
///
/// Moves via a kinematic Rigidbody rather than raw transform.position edits. This
/// isn't required for PlayerController's platform-following (that's handled purely
/// by position deltas), but it matters for collision quality: a kinematic Rigidbody
/// moved with MovePosition gets proper interpolation and swept collision detection,
/// which avoids the platform visually stuttering or tunneling through the player's
/// CharacterController on fast vertical moves.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour
{
    [Tooltip("Waypoints the platform travels between, in order. Needs at least 2.")]
    [SerializeField] private Transform[] waypoints;

    [SerializeField] private float speed = 3f;

    [Tooltip("Seconds to pause at each waypoint before continuing.")]
    [SerializeField] private float pauseAtWaypoint = 0f;

    private Rigidbody rb;
    private int targetIndex = 1;
    private float pauseTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        if (waypoints != null && waypoints.Length > 0)
            rb.position = waypoints[0].position;
    }

    private void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector3 target = waypoints[targetIndex].position;
        Vector3 next = Vector3.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        rb.MovePosition(next);

        if (Vector3.Distance(next, target) < 0.001f)
        {
            targetIndex = (targetIndex + 1) % waypoints.Length;
            pauseTimer = pauseAtWaypoint;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Transform next = waypoints[(i + 1) % waypoints.Length];
            if (next != null) Gizmos.DrawLine(waypoints[i].position, next.position);
            Gizmos.DrawWireSphere(waypoints[i].position, 0.2f);
        }
    }
#endif
}