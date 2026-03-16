using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class StrokeMeshGenerator : MonoBehaviour
{
    private Mesh strokeMesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();

    [Header("Fabric Settings")]
    [Tooltip("Maximum width of the fabric ribbon in meters")]
    [SerializeField] private float maxFabricWidth = 0.08f; 
    [Tooltip("How many extra polygons to generate between hand frames")]
    [SerializeField, Range(1, 10)] private int smoothingResolution = 5;

    private void Awake()
    {
        strokeMesh = new Mesh { name = "RunwayFabricMesh" };
        GetComponent<MeshFilter>().mesh = strokeMesh;
    }

    // Task 5.2: The Catmull-Rom Equation
    private Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

        return 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));
    }

    // Task 5.3: Dynamic Mesh Generation
    public void UpdateMesh(Stroke stroke)
    {
        // Catmull-Rom math requires a minimum of 4 points to calculate the curve tangents
        if (stroke.Points.Count < 4) return; 

        vertices.Clear();
        triangles.Clear();
        uvs.Clear();

        for (int i = 1; i < stroke.Points.Count - 2; i++)
        {
            StrokePoint p0 = stroke.Points[i - 1];
            StrokePoint p1 = stroke.Points[i];
            StrokePoint p2 = stroke.Points[i + 1];
            StrokePoint p3 = stroke.Points[i + 2];

            for (int j = 0; j < smoothingResolution; j++)
            {
                float t = j / (float)smoothingResolution;
                
                // 1. Find the smoothed center position
                Vector3 centerPos = GetCatmullRomPosition(t, p0.Position, p1.Position, p2.Position, p3.Position);

                // 2. Calculate the dynamic width based on trigger pressure
                float currentWidth = maxFabricWidth * Mathf.Lerp(p1.Pressure, p2.Pressure, t);

                // 3. Orient the ribbon flat relative to how you are holding the controller
                Quaternion currentRot = Quaternion.Slerp(p1.Rotation, p2.Rotation, t);
                Vector3 rightDir = currentRot * Vector3.right;

                // 4. Generate the Left and Right vertices of the fabric strip
                vertices.Add(centerPos - (rightDir * currentWidth * 0.5f));
                vertices.Add(centerPos + (rightDir * currentWidth * 0.5f));

                // 5. Generate UVs for future fabric textures (Sprint 4)
                float v = (i + t) / stroke.Points.Count;
                uvs.Add(new Vector2(0, v));
                uvs.Add(new Vector2(1, v));

                // 6. Connect the vertices with Triangles
                if (vertices.Count > 2)
                {
                    int vIndex = vertices.Count - 2;
                    // Triangle 1
                    triangles.Add(vIndex - 2);
                    triangles.Add(vIndex - 1);
                    triangles.Add(vIndex);
                    // Triangle 2
                    triangles.Add(vIndex - 1);
                    triangles.Add(vIndex + 1);
                    triangles.Add(vIndex);
                }
            }
        }

        // 7. Apply to the GPU
        strokeMesh.SetVertices(vertices);
        strokeMesh.SetTriangles(triangles, 0);
        strokeMesh.SetUVs(0, uvs);
        strokeMesh.RecalculateNormals(); // Crucial so lighting bounces off the fabric correctly
    }
    public void SetFabricColor(Color newColor)
    {
        if (TryGetComponent<MeshRenderer>(out MeshRenderer rend))
        {
            
            rend.material.color = newColor; 
        }
    }
}