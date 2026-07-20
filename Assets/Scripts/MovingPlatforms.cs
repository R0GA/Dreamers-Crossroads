using UnityEngine;

public class MovingPlatforms : MonoBehaviour
{
    public enum MovementAxis
    {
        WorldX,
        WorldY,
        WorldZ,
        LocalX,
        LocalY,
        LocalZ
    }

    [Header("Movement")]
    [SerializeField] private MovementAxis movementAxis = MovementAxis.LocalX;

    [SerializeField] private float distance = 5f;
    [SerializeField] private float speed = 2f;

    [Tooltip("Moves in the opposite direction first.")]
    [SerializeField] private bool reverseDirection = false;

    private Vector3 startPosition;
    private Vector3 endPosition;

    private bool movingToEnd = true;

    private void Start()
    {
        startPosition = transform.position;

        Vector3 direction = GetDirection();

        if (reverseDirection)
            direction *= -1f;

        endPosition = startPosition + direction.normalized * distance;
    }

    private void Update()
    {
        Vector3 target = movingToEnd ? endPosition : startPosition;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            movingToEnd = !movingToEnd;
        }
    }

    private Vector3 GetDirection()
    {
        switch (movementAxis)
        {
            case MovementAxis.WorldX:
                return Vector3.right;

            case MovementAxis.WorldY:
                return Vector3.up;

            case MovementAxis.WorldZ:
                return Vector3.forward;

            case MovementAxis.LocalX:
                return transform.right;

            case MovementAxis.LocalY:
                return transform.up;

            case MovementAxis.LocalZ:
                return transform.forward;

            default:
                return Vector3.right;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            Vector3 direction = GetDirection();

            if (reverseDirection)
                direction *= -1f;

            Vector3 end = transform.position + direction.normalized * distance;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, end);
            Gizmos.DrawSphere(transform.position, 0.15f);
            Gizmos.DrawSphere(end, 0.15f);
        }
    }
}
