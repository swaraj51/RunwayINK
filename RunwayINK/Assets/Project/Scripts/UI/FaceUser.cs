using UnityEngine;

public class DynamicSubtitle : MonoBehaviour
{
    [Header("Placement Settings")]
    [Tooltip("How far away from the face should the text be?")]
    public float distance = 1.5f;

    [Tooltip("How far down from eye-level? (Negative numbers move it down)")]
    public float heightOffset = -0.25f;

    private Transform mainCamera;

    void Start()
    {
        // Find the VR Headset
        if (Camera.main != null) mainCamera = Camera.main.transform;

        if (mainCamera != null)
        {
            // 1. Calculate a point exactly 'distance' meters in front of the headset's starting direction
            // We zero out the Y forward direction so the panel doesn't spawn tilted into the ceiling or floor
            Vector3 forwardLevel = new Vector3(mainCamera.forward.x, 0, mainCamera.forward.z).normalized;
            Vector3 targetPosition = mainCamera.position + (forwardLevel * distance);
            
            // 2. Drop it down slightly below eye level so it acts like a real TV subtitle
            targetPosition.y = mainCamera.position.y + heightOffset;

            // 3. Teleport the Canvas to this perfect spot!
            transform.position = targetPosition;
        }
    }

    void LateUpdate()
    {
        // 4. Always rotate the panel to face the user's eyes perfectly
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.forward);
        }
    }
}