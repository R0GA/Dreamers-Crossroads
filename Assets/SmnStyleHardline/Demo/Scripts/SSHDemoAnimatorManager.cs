namespace SmnStyleHardline.Demo
{
    /*
 * SSH Demo Animator Manager
 * Runtime demo component handling distance-based animation LOD,
 * skinned mesh enabling, and simple prop-to-bone attachment.
 */

    using UnityEngine;


    // ==========================================================================
    // Demo Animator Manager
    // ==========================================================================

    [DisallowMultipleComponent]
    public class SSHDemoAnimatorManager : MonoBehaviour
    {
        // ======================================================================
        // Animation Configuration
        // ======================================================================

        [Header("Animation Config")]
        public string configAnimName = "Idle";

        [Range(0f, 1f)]
        public float configAnimStartOffset = 0f;


        // ======================================================================
        // LOD Distances
        // ======================================================================

        [Header("LOD Distances")]
        public float configLODAnimCutoff = 40f;
        public float configLODSkinnedCutoff = 60f;


        // ======================================================================
        // Prop Hooks
        // ======================================================================

        [Header("Prop Hooks")]
        public PropHook[] propHooks;


        // ======================================================================
        // Cached References
        // ======================================================================

        Animator animator;
        SkinnedMeshRenderer[] skinnedRenderers;
        Camera mainCam;

        bool animatorEnabled = true;
        bool skinnedEnabled = true;


        // ======================================================================
        // Unity Lifecycle
        // ======================================================================

        void Awake()
        {
            animator = GetComponent<Animator>();
            skinnedRenderers =
                GetComponentsInChildren<SkinnedMeshRenderer>(true);
            mainCam = Camera.main;

            if (animator != null &&
                !string.IsNullOrEmpty(configAnimName))
            {
                animator.Play(
                    configAnimName,
                    0,
                    Mathf.Clamp01(configAnimStartOffset)
                );
            }

            if (propHooks != null)
            {
                for (int i = 0; i < propHooks.Length; i++)
                {
                    if (propHooks[i] != null)
                        propHooks[i].Attach();
                }
            }
        }

        void Update()
        {
            if (mainCam == null)
            {
                mainCam = Camera.main;
                if (mainCam == null)
                    return;
            }

            float sqrDist =
                (mainCam.transform.position - transform.position)
                .sqrMagnitude;

            float animCutoffSqr =
                configLODAnimCutoff * configLODAnimCutoff;
            float skinCutoffSqr =
                configLODSkinnedCutoff * configLODSkinnedCutoff;

            bool shouldAnimBeEnabled =
                sqrDist <= animCutoffSqr;

            if (animator != null &&
                animatorEnabled != shouldAnimBeEnabled)
            {
                animatorEnabled = shouldAnimBeEnabled;
                animator.enabled = animatorEnabled;
            }

            bool shouldSkinBeEnabled =
                sqrDist <= skinCutoffSqr;

            if (skinnedEnabled != shouldSkinBeEnabled)
            {
                skinnedEnabled = shouldSkinBeEnabled;

                for (int i = 0; i < skinnedRenderers.Length; i++)
                {
                    if (skinnedRenderers[i] != null)
                        skinnedRenderers[i].enabled = skinnedEnabled;
                }
            }
        }

        void LateUpdate()
        {
            if (propHooks == null)
                return;

            for (int i = 0; i < propHooks.Length; i++)
            {
                if (propHooks[i] != null)
                    propHooks[i].UpdateFollow();
            }
        }

        void OnDestroy()
        {
            if (propHooks == null)
                return;

            for (int i = 0; i < propHooks.Length; i++)
            {
                if (propHooks[i] != null)
                    propHooks[i].Detach();
            }
        }


        // ======================================================================
        // Prop Hook
        // ======================================================================

        [System.Serializable]
        public class PropHook
        {
            public Transform subjectProp;
            public Transform targetBone;

            [Header("Offsets (Bone Space)")]
            public Vector3 positionOffset;
            public Vector3 rotationOffsetEuler;

            Vector3 originalWorldPos;
            Quaternion originalWorldRot;
            bool hooked;

            Quaternion RotationOffset =>
                Quaternion.Euler(rotationOffsetEuler);


            // ==================================================================
            // Lifecycle
            // ==================================================================

            public void Attach()
            {
                if (hooked)
                    return;

                if (subjectProp == null ||
                    targetBone == null)
                    return;

                originalWorldPos = subjectProp.position;
                originalWorldRot = subjectProp.rotation;

                hooked = true;
                UpdateFollow();
            }

            public void UpdateFollow()
            {
                if (!hooked)
                    return;

                if (subjectProp == null ||
                    targetBone == null)
                    return;

                subjectProp.SetPositionAndRotation(
                    targetBone.TransformPoint(positionOffset),
                    targetBone.rotation * RotationOffset
                );
            }

            public void Detach()
            {
                if (!hooked || subjectProp == null)
                    return;

                subjectProp.position = originalWorldPos;
                subjectProp.rotation = originalWorldRot;

                hooked = false;
            }
        }
    }

}