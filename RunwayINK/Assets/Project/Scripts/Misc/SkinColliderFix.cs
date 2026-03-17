using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class SkinColliderFix : MonoBehaviour
{
    private void Start()
    {
        SkinnedMeshRenderer skin = GetComponent<SkinnedMeshRenderer>();
        MeshCollider col = GetComponent<MeshCollider>();

        // Create an empty mesh
        Mesh bakedMesh = new Mesh();
        
        // Force the SkinnedMeshRenderer to calculate its exact current pose and bake it into the empty mesh
        skin.BakeMesh(bakedMesh);
        
        // Assign that perfectly matched mesh to our physics collider!
        col.sharedMesh = bakedMesh;
    }
}