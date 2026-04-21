using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class SkinColliderFix : MonoBehaviour
{
    private SkinnedMeshRenderer skin;
    private MeshCollider col;
    private Mesh bakedMesh;

    private void Start()
    {
        skin = GetComponent<SkinnedMeshRenderer>();
        col = GetComponent<MeshCollider>();
        bakedMesh = new Mesh(); // Create the mesh once in memory
    }

    // LateUpdate is CRITICAL. It waits for the Animator to move the bones 
    // before we bake the new collider shape.
    private void LateUpdate()
    {
        if (skin != null && col != null)
        {
            // 1. Take a snapshot of the walking animation's current pose
            skin.BakeMesh(bakedMesh);
            
            // 2. Immediately update the collider to match that pose
            col.sharedMesh = null; // High-speed refresh trick
            col.sharedMesh = bakedMesh;
        }
    }
}