using UnityEngine;

public class DrawingEngine : MonoBehaviour, IDrawingEngine 
{
    [Header("Visual Feedback")]
    [SerializeField] private Transform stylusCursor;
    [SerializeField] private float maxCursorSize = 0.02f;
    [Header("Tool States")]
    [Tooltip("Check this box to test the freehand Drape Tool. Uncheck for Sketch Tool.")]
    public bool isDrapeMode = false; 
    
    //[Header("Generation")]
    //[Tooltip("Drag your FabricStroke_Prefab here from the project folder")]
   //[SerializeField] private StrokeMeshGenerator strokePrefab; 

    [Header("Surface Projection")]
    [Tooltip("The physics layer for the Mannequin")]
    [SerializeField] private LayerMask canvasLayer;
    
    [Tooltip("How far the pen can be before it snaps to the body (in meters)")]
    [SerializeField] private float snapDistance = 0.1f; // 10 cm
    
    [Tooltip("Pushes the fabric off the skin so it doesn't clip inside")]
    [SerializeField] private float skinOffset = 0.005f; // 5 millimeters

    [Header("Croquis Boundaries (Phase 1)")]
    [Tooltip("Drag your new BoundaryLine_Prefab here")]
    [SerializeField] private LineRenderer boundaryLinePrefab;
    [Header("Spline Editor (Phase 3)")]
    [Tooltip("How close the pen needs to be to grab a point (in meters)")]
    [SerializeField] private float grabRadius = 0.05f; // 5 cm
    
    // Memory bank for all completed lines
    private System.Collections.Generic.List<LineRenderer> allDrawnLines = new System.Collections.Generic.List<LineRenderer>();
    // Memory bank for the fabrics we generate
    private System.Collections.Generic.List<GameObject> generatedFabrics = new System.Collections.Generic.List<GameObject>();
    // Variables to remember exactly which point we are dragging
    private LineRenderer grabbedLine = null;
    private int grabbedPointIndex = -1;
    
    // This holds the current line we are actively drawing
    private LineRenderer currentBoundaryLine;
    private Stroke currentStroke;
    private StrokeMeshGenerator currentMeshGenerator;
    private Color currentColor = Color.cyan;
    private bool wasDrawingLastFrame = false;
    private Renderer cursorRenderer;


   //Testing 
   private LineRenderer vrDebugLaser;

    private void Start()
    {
        vrDebugLaser = new GameObject("VRLaser").AddComponent<LineRenderer>();
        vrDebugLaser.startWidth = 0.005f; 
        vrDebugLaser.endWidth = 0.005f;
        vrDebugLaser.material = new Material(Shader.Find("Sprites/Default"));
        if (stylusCursor != null)
        {
            cursorRenderer = stylusCursor.GetComponent<Renderer>();
            
            // Set the cursor to match your default starting color (Cyan)
            if (cursorRenderer != null) 
            {
                cursorRenderer.material.color = currentColor;
            }
        }
    }
    private void UpdateCursorVisuals(Vector3 position, Quaternion rotation, float pressure)
    {
        if (stylusCursor == null) return;
        stylusCursor.position = position;
        stylusCursor.rotation = rotation;
        float currentSize = Mathf.Lerp(0.005f, maxCursorSize, pressure);
        stylusCursor.localScale = new Vector3(currentSize, currentSize, currentSize);
    }

   public void ProcessInput(Vector3 position, Quaternion rotation, float pressure, bool isDrawingNow)
    {
        Vector3 forwardDirection = rotation * Vector3.forward; 
        Vector3 rayStart = position - (forwardDirection * 0.02f); 

        vrDebugLaser.SetPosition(0, rayStart);
        bool hitCanvas = Physics.Raycast(rayStart, forwardDirection, out RaycastHit hit, snapDistance, canvasLayer);
        Vector3 snappedPosition = hitCanvas ? hit.point + (hit.normal * skinOffset) : position; 

        // Laser visuals
        vrDebugLaser.SetPosition(1, hitCanvas ? hit.point : rayStart + (forwardDirection * snapDistance));
        vrDebugLaser.startColor = hitCanvas ? Color.green : Color.red;
        vrDebugLaser.endColor = hitCanvas ? Color.green : Color.red;

        // 1. TRIGGER PULLED
        if (isDrawingNow && !wasDrawingLastFrame && hitCanvas)
        {
            if (!isDrapeMode) 
            {
                // TOOL 1: PENCIL - Start a new white line
                currentBoundaryLine = Instantiate(boundaryLinePrefab, Vector3.zero, Quaternion.identity);
                currentBoundaryLine.positionCount = 1;
                currentBoundaryLine.SetPosition(0, snappedPosition);
            }
            else 
            {
                // TOOL 2: FABRIC MARKER - Paint Bucket Fill!
                FillClosestLoop(snappedPosition);
            }
        }
        
        // 2. HOLDING TRIGGER
        else if (isDrawingNow && wasDrawingLastFrame && hitCanvas)
        {
            if (!isDrapeMode)
            {
                // CRITICAL FIX: If you pulled the trigger in the air and THEN touched the body, create the line!
                if (currentBoundaryLine == null)
                {
                    currentBoundaryLine = Instantiate(boundaryLinePrefab, Vector3.zero, Quaternion.identity);
                    currentBoundaryLine.positionCount = 1;
                    currentBoundaryLine.SetPosition(0, snappedPosition);
                }

                Vector3 lastPoint = currentBoundaryLine.GetPosition(currentBoundaryLine.positionCount - 1);
                
                // Drop a new point if we moved far enough
                if (Vector3.Distance(lastPoint, snappedPosition) > 0.005f) 
                {
                    currentBoundaryLine.positionCount++;
                    currentBoundaryLine.SetPosition(currentBoundaryLine.positionCount - 1, snappedPosition);
                }
            }
        }
        
        // 3. TRIGGER RELEASED
        else if ((!isDrawingNow || !hitCanvas) && wasDrawingLastFrame)
        {
            if (!isDrapeMode && currentBoundaryLine != null)
            {
                currentBoundaryLine.Simplify(0.002f); 
                int pointCount = currentBoundaryLine.positionCount;

                // CRITICAL FIX RESTORED: The "Tail Cutter" to snap shapes closed perfectly!
                if (pointCount > 10) 
                {
                    bool loopClosed = false;
                    int cutStartIndex = 0;
                    int cutEndIndex = pointCount - 1;
                    int searchRange = Mathf.Max(5, Mathf.RoundToInt(pointCount * 0.3f));

                    for (int i = pointCount - 1; i >= pointCount - searchRange; i--) 
                    {
                        for (int j = 0; j < searchRange; j++) 
                        {
                            if (Vector3.Distance(currentBoundaryLine.GetPosition(i), currentBoundaryLine.GetPosition(j)) < 0.03f) 
                            {
                                cutStartIndex = j;
                                cutEndIndex = i;
                                loopClosed = true;
                                break; 
                            }
                        }
                        if (loopClosed) break; 
                    }

                    if (loopClosed) 
                    {
                        int newPointCount = (cutEndIndex - cutStartIndex) + 1;
                        Vector3[] trimmedPoints = new Vector3[newPointCount];

                        for (int k = 0; k < newPointCount; k++) 
                        {
                            trimmedPoints[k] = currentBoundaryLine.GetPosition(cutStartIndex + k);
                        }

                        trimmedPoints[newPointCount - 1] = trimmedPoints[0]; 

                        currentBoundaryLine.positionCount = newPointCount;
                        currentBoundaryLine.SetPositions(trimmedPoints);
                        Debug.Log("Pencil Tool: Loop perfectly closed!");
                    }
                }
                
                // Save the white outline permanently!
                allDrawnLines.Add(currentBoundaryLine);
                currentBoundaryLine = null;
            }
        }
        wasDrawingLastFrame = isDrawingNow;
    }

   public void SetColor(Color color) 
    {
        currentColor = color;
        
        // INSTANT VISUAL FEEDBACK: Paint the cursor sphere!
        if (cursorRenderer != null)
        {
            cursorRenderer.material.color = currentColor;
        }
    }
    public void ClearAllStrokes() { /* Sprint 5 */ }
    public void ProcessEditInput(Vector3 penPosition, bool isHoldingEditButton)
    {
        if (isHoldingEditButton)
        {
            // 1. If we aren't holding anything yet, search for the closest point
            if (grabbedLine == null)
            {
                float closestDistance = grabRadius; // Start with our max grab range

                foreach (LineRenderer line in allDrawnLines)
                {
                    if (line == null) continue;

                    // Check every single point on this line
                    for (int i = 0; i < line.positionCount; i++)
                    {
                        float dist = Vector3.Distance(penPosition, line.GetPosition(i));
                        if (dist < closestDistance)
                        {
                            closestDistance = dist;
                            grabbedLine = line;
                            grabbedPointIndex = i;
                        }
                    }
                }
            }

            // 2. If we successfully grabbed a point, move it to the pen tip!
            if (grabbedLine != null && grabbedPointIndex != -1)
            {
                grabbedLine.SetPosition(grabbedPointIndex, penPosition);
                
                // Visual feedback: Turn the laser yellow while dragging
                vrDebugLaser.startColor = Color.yellow;
                vrDebugLaser.endColor = Color.yellow;
            }
        }
        else
        {
            // 3. Button released. Let go of the point.
            grabbedLine = null;
            grabbedPointIndex = -1;
        }
    }
    // --- THE PAINT BUCKET TOOL ---
    private void FillClosestLoop(Vector3 penPosition)
    {
        if (allDrawnLines.Count == 0) return;

        LineRenderer closestLine = null;
        float closestDistance = grabRadius; // Uses your existing grab radius (e.g., 5cm)

        // Find the closest white outline
        foreach (LineRenderer line in allDrawnLines)
        {
            if (line == null) continue;
            for (int i = 0; i < line.positionCount; i++)
            {
                float dist = Vector3.Distance(penPosition, line.GetPosition(i));
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestLine = line;
                }
            }
        }

        // If we found a line, fill it with fabric!
        if (closestLine != null)
        {
            FillLoopWithFabric(closestLine);
            Debug.Log("Paint Bucket: Filled the white outline with Fabric!");
        }
    }
    private void FillLoopWithFabric(LineRenderer boundaryLine)
    {
        int pointCount = boundaryLine.positionCount;
        if (pointCount < 3) return; // We need at least 3 points to make a shape!

        // 1. Extract all the points from your spatial marker line
        Vector3[] boundaryPoints = new Vector3[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            boundaryPoints[i] = boundaryLine.GetPosition(i);
        }

        // 2. Find the mathematical center (The hub of the fan)
        Vector3 centerPoint = Vector3.zero;
        foreach (Vector3 pt in boundaryPoints)
        {
            centerPoint += pt;
        }
        centerPoint /= pointCount;

        // 3. Prepare the Mesh Vertices array
        Vector3[] vertices = new Vector3[pointCount + 1];
        vertices[0] = centerPoint; // The center point is always Index 0
        
        for (int i = 0; i < pointCount; i++)
        {
            vertices[i + 1] = boundaryPoints[i];
        }

        // 4. Create the Triangles (Connecting the pizza slices)
        // 4. Create the Triangles (Double-Sided!)
        // We multiply by 6 instead of 3, because we need two triangles per slice
        int[] triangles = new int[pointCount * 6]; 
        for (int i = 0; i < pointCount; i++)
        {
            int currentEdge = i + 1;
            int nextEdge = (i + 1) % pointCount + 1; 

            // Front Face
            triangles[i * 6] = 0; 
            triangles[i * 6 + 1] = currentEdge; 
            triangles[i * 6 + 2] = nextEdge; 

            // Back Face (We simply swap the last two numbers to reverse the facing direction!)
            triangles[i * 6 + 3] = 0; 
            triangles[i * 6 + 4] = nextEdge; 
            triangles[i * 6 + 5] = currentEdge; 
        }

        // 5. Build the physical Mesh
        Mesh fabricMesh = new Mesh();
        fabricMesh.vertices = vertices;
        fabricMesh.triangles = triangles;
        fabricMesh.RecalculateNormals(); // This makes the fabric react to lighting!

        // 6. Spawn the physical object into the scene
        GameObject fabricObject = new GameObject("FabricPatch");
        generatedFabrics.Add(fabricObject);
        MeshFilter meshFilter = fabricObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = fabricObject.AddComponent<MeshRenderer>();
        
        meshFilter.mesh = fabricMesh;
        
        // Assign a default URP material so we can see it clearly
        meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        meshRenderer.material.color = Color.cyan; 
        
    }
    // --- MVP FEATURE 1: UNDO & CLEAR ---
    public void UndoLastOutline()
    {
        if (allDrawnLines.Count > 0)
        {
            LineRenderer lastLine = allDrawnLines[allDrawnLines.Count - 1];
            allDrawnLines.RemoveAt(allDrawnLines.Count - 1);
            if (lastLine != null) Destroy(lastLine.gameObject);
            Debug.Log("Undo: Removed last white outline.");
        }
    }

    public void UndoLastFabric()
    {
        if (generatedFabrics.Count > 0)
        {
            GameObject lastFabric = generatedFabrics[generatedFabrics.Count - 1];
            generatedFabrics.RemoveAt(generatedFabrics.Count - 1);
            if (lastFabric != null) Destroy(lastFabric);
            Debug.Log("Undo: Removed last fabric patch.");
        }
    }

    public void ClearEntireCanvas()
    {
        foreach (var line in allDrawnLines) { if (line != null) Destroy(line.gameObject); }
        allDrawnLines.Clear();
        
        foreach (var fabric in generatedFabrics) { if (fabric != null) Destroy(fabric); }
        generatedFabrics.Clear();
        Debug.Log("Cleared the entire mannequin!");
    }
    // --- PHASE 3B: THE FREEHAND RIBBON BRUSH ---
    /*private void GenerateRibbonFabric(LineRenderer line, float fabricWidth = 0.04f)
    {
        int pointCount = line.positionCount;
        if (pointCount < 2) return; // We just need a start and end point to make a ribbon

        Vector3[] points = new Vector3[pointCount];
        line.GetPositions(points);

        // We need 2 vertices (Left and Right) for every point on your line
        Vector3[] vertices = new Vector3[pointCount * 2];
        
        // We need 2 triangles (a square) to connect every step, multiplied by 2 for double-sided!
        int[] triangles = new int[(pointCount - 1) * 12]; 

        for (int i = 0; i < pointCount; i++)
        {
            // Figure out which way the pen is moving
            Vector3 forward = Vector3.zero;
            if (i < pointCount - 1) forward = (points[i + 1] - points[i]).normalized;
            else forward = (points[i] - points[i - 1]).normalized;

            // Figure out which way is "Outward" so the fabric lies flat against the mannequin
            Vector3 outwardNormal = (Camera.main.transform.position - points[i]).normalized;
            
            // Calculate the Left and Right edges of the ribbon
            Vector3 right = Vector3.Cross(forward, outwardNormal).normalized;

            vertices[i * 2] = points[i] + (right * fabricWidth * 0.5f);     // Left edge
            vertices[i * 2 + 1] = points[i] - (right * fabricWidth * 0.5f); // Right edge
        }

        // Stitch the edges together with triangles
        int triIndex = 0;
        for (int i = 0; i < pointCount - 1; i++)
        {
            int v0 = i * 2;         // Bottom Left
            int v1 = i * 2 + 1;     // Bottom Right
            int v2 = (i + 1) * 2;   // Top Left
            int v3 = (i + 1) * 2 + 1; // Top Right

            // Front Face of the fabric
            triangles[triIndex++] = v0; triangles[triIndex++] = v2; triangles[triIndex++] = v1;
            triangles[triIndex++] = v1; triangles[triIndex++] = v2; triangles[triIndex++] = v3;

            // Back Face of the fabric (Double-Sided)
            triangles[triIndex++] = v0; triangles[triIndex++] = v1; triangles[triIndex++] = v2;
            triangles[triIndex++] = v1; triangles[triIndex++] = v3; triangles[triIndex++] = v2;
        }

        Mesh fabricMesh = new Mesh();
        fabricMesh.vertices = vertices;
        fabricMesh.triangles = triangles;
        fabricMesh.RecalculateNormals();

        GameObject fabricObject = new GameObject("FreehandRibbon");
        MeshFilter meshFilter = fabricObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = fabricObject.AddComponent<MeshRenderer>();
        
        meshFilter.mesh = fabricMesh;
        meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        meshRenderer.material.color = currentColor; // Uses your globally selected color!// Making this pink so you can tell the difference!
    }*/
}