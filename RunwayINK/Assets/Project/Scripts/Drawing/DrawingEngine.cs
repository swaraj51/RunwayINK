using UnityEngine;

public class DrawingEngine : MonoBehaviour, IDrawingEngine 
{
    [Header("Visual Feedback")]
    [SerializeField] private Transform stylusCursor;
    [SerializeField] private float maxCursorSize = 0.02f;
    [Header("Tool States")]
    [Tooltip("Check this box to test the freehand Drape Tool. Uncheck for Sketch Tool.")]
    public bool isDrapeMode = false; 
    [Header("Surface Projection")]
    [Tooltip("The physics layer for the Mannequin")]
    [SerializeField] private LayerMask canvasLayer;
    
    [Tooltip("How far the pen can be before it snaps to the body (in meters)")]
    [SerializeField] private float snapDistance = 0.1f; // 10 cm
    
    [Tooltip("Pushes the fabric off the skin so it doesn't clip inside")]
    [SerializeField] private float skinOffset = 0.005f; // 5 millimeters
    [Header("Runway Setup")]
    [Tooltip("Drag your Mannequin here so clothes stick to her!")]
    public Transform mannequinTransform;
    [Tooltip("Drag the 'Armature', 'Rig', or 'Hips' from inside the Mannequin here")]
    public Transform mannequinArmature;
    private Transform[] allBones;
    
    [Header("Croquis Boundaries (Phase 1)")]
    [Tooltip("Drag your new BoundaryLine_Prefab here")]
    [SerializeField] private LineRenderer boundaryLinePrefab;
    [Header("Spline Editor (Phase 3)")]
    [Tooltip("How close the pen needs to be to grab a point (in meters)")]
    [SerializeField] private float grabRadius = 0.05f; // 5 cm
    [Header("Fabric System")]
    public Material currentFabricMaterial;
    [Tooltip("Drag a new LineRenderer prefab here, set width to 0.05 for a thick brush")]
    public LineRenderer fabricBrushPrefab;
    private System.Collections.Generic.List<LineRenderer> allDrawnLines = new System.Collections.Generic.List<LineRenderer>();
    // Memory bank for the fabrics we generate
    private System.Collections.Generic.List<GameObject> generatedFabrics = new System.Collections.Generic.List<GameObject>();
    // Memory bank for the fabrics we generate
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
        vrDebugLaser.startWidth = 0.002f; // Make it very thin and professional
        vrDebugLaser.endWidth = 0.002f;
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
        if (mannequinArmature != null) 
        {
            allBones = mannequinArmature.GetComponentsInChildren<Transform>();
        }
        
    }
   private void UpdateCursorVisuals(Vector3 position, Quaternion rotation, float pressure)
    {
        if (stylusCursor == null) return;
        stylusCursor.position = position;
        stylusCursor.rotation = rotation;
        
       
    }

    public void ProcessInput(Vector3 position, Quaternion rotation, float pressure, bool isDrawingNow)
    {
        Vector3 forwardDirection = rotation * Vector3.forward; 
        Vector3 rayStart = position - (forwardDirection * 0.02f); 

        // 1. RAYCAST: Find the skin
        bool hitCanvas = Physics.Raycast(rayStart, forwardDirection, out RaycastHit hit, snapDistance, canvasLayer);
        
        // THE FIX: Give thick fabrics an extra 5 millimeters of padding so they don't clip into curved body parts!
        float dynamicSkinOffset = isDrapeMode ? (skinOffset + 0.005f) : skinOffset; 
        Vector3 snappedPosition = hitCanvas ? hit.point + (hit.normal * dynamicSkinOffset) : position;

        // 2. SMART LASER: Visual feedback
        if (vrDebugLaser != null) {
            vrDebugLaser.SetPosition(0, rayStart);
            if (hitCanvas) {
                vrDebugLaser.SetPosition(1, hit.point);
                vrDebugLaser.startColor = Color.cyan;
                vrDebugLaser.endColor = Color.cyan;
                if (stylusCursor != null) stylusCursor.gameObject.SetActive(true);
            } else {
                vrDebugLaser.SetPosition(1, rayStart + (forwardDirection * 0.05f));
                vrDebugLaser.startColor = new Color(1, 1, 1, 0.3f);
                vrDebugLaser.endColor = new Color(1, 1, 1, 0.3f);
                if (stylusCursor != null) stylusCursor.gameObject.SetActive(false);
            }
        }

        // 3. TRIGGER PULLED: Start the Line and Glue to Bone
        if (isDrawingNow && !wasDrawingLastFrame && hitCanvas)
        {
            Transform targetBone = GetNearestBone(snappedPosition);
            
            if (!isDrapeMode) 
            {
                currentBoundaryLine = Instantiate(boundaryLinePrefab, Vector3.zero, Quaternion.identity);
                currentBoundaryLine.transform.SetParent(targetBone, false); 
                currentBoundaryLine.positionCount = 1;
                // LOCAL SPACE CONVERSION
                currentBoundaryLine.SetPosition(0, targetBone.InverseTransformPoint(snappedPosition)); 

                currentBoundaryLine.startColor = currentColor;
                currentBoundaryLine.endColor = currentColor;
                if (currentBoundaryLine.material.HasProperty("_BaseColor")) 
                    currentBoundaryLine.material.SetColor("_BaseColor", currentColor);
            }
            else 
            {
                currentBoundaryLine = Instantiate(fabricBrushPrefab, Vector3.zero, Quaternion.identity);
                currentBoundaryLine.transform.SetParent(targetBone, false); 
                currentBoundaryLine.positionCount = 1;
                // LOCAL SPACE CONVERSION
                currentBoundaryLine.SetPosition(0, targetBone.InverseTransformPoint(snappedPosition));

                if (currentFabricMaterial != null) currentBoundaryLine.material = new Material(currentFabricMaterial);
                else currentBoundaryLine.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                
                currentBoundaryLine.startColor = currentColor;
                currentBoundaryLine.endColor = currentColor;
                if (currentBoundaryLine.material.HasProperty("_BaseColor")) 
                    currentBoundaryLine.material.SetColor("_BaseColor", currentColor);
            }
        }
        
        // 4. HOLDING TRIGGER: Continue drawing the Line across the bone
        // *** THIS IS THE PART YOU WERE MISSING! DO NOT DELETE THIS! ***
        else if (isDrawingNow && wasDrawingLastFrame && hitCanvas && currentBoundaryLine != null)
        {
            Transform parentBone = currentBoundaryLine.transform.parent; 
            Vector3 localSnappedPos = parentBone.InverseTransformPoint(snappedPosition);
            
            Vector3 lastLocalPoint = currentBoundaryLine.GetPosition(currentBoundaryLine.positionCount - 1);
            if (Vector3.Distance(lastLocalPoint, localSnappedPos) > 0.005f) 
            {
                currentBoundaryLine.positionCount++;
                currentBoundaryLine.SetPosition(currentBoundaryLine.positionCount - 1, localSnappedPos);
            }
        }

        // 5. TRIGGER RELEASED: Stop drawing and save to memory
        else if ((!isDrawingNow || !hitCanvas) && wasDrawingLastFrame && currentBoundaryLine != null)
        {
            if (!isDrapeMode) 
            {
                // PENCIL: Smooth it out to remove hand jitter
                currentBoundaryLine.Simplify(0.002f); 
                allDrawnLines.Add(currentBoundaryLine);
            }
            else 
            {
                // FABRIC: Do NOT simplify! Leave the raw points exactly where you drew them.
                generatedFabrics.Add(currentBoundaryLine.gameObject); 
            }
            
            currentBoundaryLine = null;
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
        
       // Use the selected fabric texture, or fallback to default if none is selected
        if (currentFabricMaterial != null) 
            meshRenderer.material = new Material(currentFabricMaterial);
        else 
            meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            
        // CRITICAL: Apply the dipped color to the fabric!
        meshRenderer.material.color = currentColor;
        
    }
    public void UndoLastAction()
    {
        // If we are currently holding the Fabric Marker (Paint Bucket)
        if (isDrapeMode)
        {
            if (generatedFabrics.Count > 0)
            {
                GameObject lastFabric = generatedFabrics[generatedFabrics.Count - 1];
                generatedFabrics.RemoveAt(generatedFabrics.Count - 1);
                if (lastFabric != null) Destroy(lastFabric);
                Debug.Log("Smart Undo: Removed the last 3D Fabric.");
            }
        }
        // If we are currently holding the Pencil (White Outlines)
        else
        {
            if (allDrawnLines.Count > 0)
            {
                LineRenderer lastLine = allDrawnLines[allDrawnLines.Count - 1];
                allDrawnLines.RemoveAt(allDrawnLines.Count - 1);
                if (lastLine != null) Destroy(lastLine.gameObject);
                Debug.Log("Smart Undo: Removed the last Pencil Outline.");
            }
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
    // --- BONE SCANNER MATH ---
    private Transform GetNearestBone(Vector3 hitPoint)
    {
        // Failsafe: If no skeleton is assigned, fallback to the main body!
        if (allBones == null || allBones.Length == 0) return mannequinTransform;
        
        Transform nearest = mannequinTransform;
        float minDistance = Mathf.Infinity;
        
        // Loop through every bone in the body and find the closest one
        foreach (Transform bone in allBones)
        {
            float dist = Vector3.Distance(bone.position, hitPoint);
            if (dist < minDistance) 
            { 
                minDistance = dist; 
                nearest = bone; 
            }
        }
        return nearest;
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