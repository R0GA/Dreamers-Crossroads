namespace SmnStyleHardline.Demo
{
    /*
     * SSH Demo Cheap Boid
     * Lightweight, deterministic boid-style simulation intended for
     * background motion, ambience, and visual testing.
     */

    using UnityEngine;


    // ==========================================================================
    // Demo Cheap Boid
    // ==========================================================================

    public class SSHDemoCheapBoid : MonoBehaviour
    {
        // ======================================================================
        // Boids
        // ======================================================================

        [Header("Boids")]
        public Transform[] boids;


        // ======================================================================
        // Simulation
        // ======================================================================

        [Header("Simulation")]
        public float cycleDuration = 12f;
        public Vector3 boundSize = new Vector3(20f, 10f, 20f);
        public float maxSpeed = 3.5f;
        public float turnSpeed = 4f;


        // ======================================================================
        // Behavior Weights
        // ======================================================================

        [Header("Behavior Weights")]
        public float centerAttraction = 1.2f;
        public float separationStrength = 1.6f;
        public float noiseStrength = 0.6f;


        // ======================================================================
        // Axis Lock
        // ======================================================================

        [Header("Loose Axis Lock (Local Space)")]
        public Vector3 looseLockStrength = new Vector3(0f, 2.5f, 0f);


        // ======================================================================
        // Speed Variation
        // ======================================================================

        [Header("Speed Variation")]
        [Range(0f, 1f)]
        public float speedVariation = 0.35f;


        // ======================================================================
        // Shallow Water Behavior
        // ======================================================================

        [Header("Shallow Water Behavior")]
        public float verticalVelocityDamping = 6f;
        public float planarSteeringStrength = 3f;
        public float pitchRelaxStrength = 4f;


        // ======================================================================
        // Determinism
        // ======================================================================

        [Header("Determinism")]
        public int seed = 0;


        // ======================================================================
        // State
        // ======================================================================

        Vector3[] velocities;
        Vector3[] noiseSeeds;
        float[] speedMultipliers;

        float time;
        Vector3 halfBounds;
        int count;


        // ======================================================================
        // Unity Lifecycle
        // ======================================================================

        void Awake()
        {
            count = boids.Length;

            velocities = new Vector3[count];
            noiseSeeds = new Vector3[count];
            speedMultipliers = new float[count];

            halfBounds = boundSize * 0.5f;

            float Hash01(int i) =>
                Mathf.Abs(Mathf.Sin((i + seed * 17) * 12.9898f)) % 1f;

            float HashSigned(int i) =>
                Hash01(i) * 2f - 1f;

            for (int i = 0; i < count; i++)
            {
                float speedT =
                    Mathf.Abs(Mathf.Sin((i + seed) * 2.17f));

                speedMultipliers[i] = Mathf.Lerp(
                    1f - speedVariation,
                    1f + speedVariation,
                    speedT
                );

                Vector3 localOffset = new Vector3(
                    HashSigned(i * 3) * halfBounds.x,
                    HashSigned(i * 5) * halfBounds.y,
                    HashSigned(i * 7) * halfBounds.z
                );

                boids[i].position =
                    transform.TransformPoint(localOffset);

                Vector3 dir = new Vector3(
                    HashSigned(i * 11),
                    HashSigned(i * 13) * 0.3f,
                    HashSigned(i * 17)
                ).normalized;

                velocities[i] =
                    dir * maxSpeed * speedMultipliers[i];

                noiseSeeds[i] = new Vector3(
                    (i + seed) * 1.17f,
                    (i + seed) * 2.31f,
                    (i + seed) * 3.91f
                );

                if (velocities[i].sqrMagnitude > 0.001f)
                    boids[i].forward = velocities[i].normalized;
            }
        }

        void Update()
        {
            if (count == 0)
                return;

            time += Time.deltaTime;

            float cycleT =
                time % cycleDuration / cycleDuration;
            float loopAngle =
                cycleT * Mathf.PI * 2f;

            Transform root = transform;

            Vector3 localCenter = Vector3.zero;
            for (int i = 0; i < count; i++)
                localCenter +=
                    root.InverseTransformPoint(boids[i].position);
            localCenter /= count;

            for (int i = 0; i < count; i++)
            {
                Transform boid = boids[i];

                float boidMaxSpeed =
                    maxSpeed * speedMultipliers[i];
                float boidTurnSpeed =
                    turnSpeed / speedMultipliers[i];

                Vector3 localPos =
                    root.InverseTransformPoint(boid.position);
                Vector3 localVel =
                    root.InverseTransformDirection(velocities[i]);

                Vector3 toCenter =
                    (localCenter - localPos) * centerAttraction;

                Vector3 separation = Vector3.zero;
                float closestDist = float.MaxValue;

                for (int j = 0; j < count; j++)
                {
                    if (j == i)
                        continue;

                    Vector3 otherLocal =
                        root.InverseTransformPoint(
                            boids[j].position);

                    Vector3 d = localPos - otherLocal;
                    float sqr = d.sqrMagnitude;

                    if (sqr < closestDist)
                    {
                        closestDist = sqr;
                        separation = d;
                    }
                }

                separation *=
                    separationStrength / (closestDist + 0.001f);

                Vector3 seedVec = noiseSeeds[i];
                Vector3 noise = new Vector3(
                    Mathf.Sin(loopAngle + seedVec.x),
                    Mathf.Cos(loopAngle * 1.3f + seedVec.y),
                    Mathf.Sin(loopAngle * 0.7f + seedVec.z)
                ) * noiseStrength;

                Vector3 axisLock = new Vector3(
                    -localPos.x * looseLockStrength.x,
                    -localPos.y * looseLockStrength.y,
                    -localPos.z * looseLockStrength.z
                );

                Vector3 desiredLocal =
                    localVel +
                    (toCenter + separation + noise + axisLock)
                    * Time.deltaTime;

                desiredLocal.y = Mathf.Lerp(
                    desiredLocal.y,
                    0f,
                    Time.deltaTime * verticalVelocityDamping
                );

                Vector3 planar =
                    new Vector3(desiredLocal.x, 0f, desiredLocal.z);

                desiredLocal = Vector3.Lerp(
                    desiredLocal,
                    planar,
                    Time.deltaTime * planarSteeringStrength
                );

                desiredLocal =
                    Vector3.ClampMagnitude(
                        desiredLocal, boidMaxSpeed);

                Vector3 boundForce = Vector3.zero;

                if (localPos.x > halfBounds.x) boundForce.x = -1;
                else if (localPos.x < -halfBounds.x) boundForce.x = 1;

                if (localPos.y > halfBounds.y) boundForce.y = -1;
                else if (localPos.y < -halfBounds.y) boundForce.y = 1;

                if (localPos.z > halfBounds.z) boundForce.z = -1;
                else if (localPos.z < -halfBounds.z) boundForce.z = 1;

                desiredLocal += boundForce * boidMaxSpeed;

                Vector3 newLocalVel =
                    Vector3.Lerp(
                        localVel,
                        desiredLocal,
                        Time.deltaTime * boidTurnSpeed
                    );

                velocities[i] =
                    root.TransformDirection(newLocalVel);

                boid.position +=
                    velocities[i] * Time.deltaTime;

                Vector3 velDir = velocities[i].normalized;
                Vector3 planarDir =
                    new Vector3(velDir.x, 0f, velDir.z).normalized;

                Vector3 relaxedDir = Vector3.Lerp(
                    velDir,
                    planarDir,
                    Time.deltaTime * pitchRelaxStrength
                );

                if (relaxedDir.sqrMagnitude > 0.001f)
                    boid.forward = relaxedDir;
            }
        }


        // ======================================================================
        // Gizmos
        // ======================================================================

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, boundSize);
        }
#endif
    }

}