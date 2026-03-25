using UnityEngine;

public class DrawingEngine : MonoBehaviour, IDrawingEngine 
{
    [Header("Visual Feedback")]
    [SerializeField] private Transform stylusCursor;
    [SerializeField] private float maxCursorSize = 0.02f; 
    
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
        //isDrawingNow = stylusInput.IsDrawing;
        
        Vector3 forwardDirection = rotation * Vector3.forward; 
        Vector3 rayStart = position - (forwardDirection * 0.02f); 

        // 1. Lock the start of the laser to your pen tip
        vrDebugLaser.SetPosition(0, rayStart);

        // --- THE SECURITY GATE ---
        // We now save the result of the raycast to a boolean so we can use it to block drawing
        bool hitCanvas = Physics.Raycast(rayStart, forwardDirection, out RaycastHit hit, snapDistance, canvasLayer);
        Vector3 snappedPosition = position; 

        if (hitCanvas)
        {
            snappedPosition = hit.point + (hit.normal * skinOffset);
            vrDebugLaser.SetPosition(1, hit.point);
            vrDebugLaser.startColor = Color.green;
            vrDebugLaser.endColor = Color.green;
        }
        else
        {
            vrDebugLaser.SetPosition(1, rayStart + (forwardDirection * snapDistance));
            vrDebugLaser.startColor = Color.red;
            vrDebugLaser.endColor = Color.red;
        }

       // 1. TRIGGER PULLED & HITTING CANVAS: Start a new boundary line
        if (isDrawingNow && !wasDrawingLastFrame && hitCanvas)
        {
            // Spawn a new line and set its very first point
            currentBoundaryLine = Instantiate(boundaryLinePrefab, Vector3.zero, Quaternion.identity);
            currentBoundaryLine.positionCount = 1;
            currentBoundaryLine.SetPosition(0, snappedPosition);
        }
        
        // 2. HOLDING TRIGGER & HITTING CANVAS: Add points to the line
        else if (isDrawingNow && wasDrawingLastFrame && hitCanvas)
        {
            // Safety check: Ensure the line actually exists
            if (currentBoundaryLine == null)
            {
                currentBoundaryLine = Instantiate(boundaryLinePrefab, Vector3.zero, Quaternion.identity);
                currentBoundaryLine.positionCount = 1;
                currentBoundaryLine.SetPosition(0, snappedPosition);
            }

            // Get the last point we drew
            Vector3 lastPoint = currentBoundaryLine.GetPosition(currentBoundaryLine.positionCount - 1);

            // Only drop a new point if we moved the pen far enough (e.g., 5 millimeters)
            // This prevents the line from having too many overlapping points, keeping performance high!
            if (Vector3.Distance(lastPoint, snappedPosition) > 0.005f) 
            {
                currentBoundaryLine.positionCount++;
                currentBoundaryLine.SetPosition(currentBoundaryLine.positionCount - 1, snappedPosition);
            }
        }
        
        // 3. TRIGGER RELEASED OR FELL OFF BODY: Finish the line
        // 3. TRIGGER RELEASED OR FELL OFF BODY: Finish the line
        else if ((!isDrawingNow || !hitCanvas) && wasDrawingLastFrame)
        {
            if (currentBoundaryLine != null)
            {
                // 1. Smooth the line out into editable spline points
                currentBoundaryLine.Simplify(0.002f); 
                
                int pointCount = currentBoundaryLine.positionCount;
                if (pointCount > 10) 
                {
                    Vector3 startPoint = currentBoundaryLine.GetPosition(0);
                    Vector3 endPoint = currentBoundaryLine.GetPosition(pointCount - 1);
                    float gapDistance = Vector3.Distance(startPoint, endPoint);
                    
                    // 2. Calculate the total length of the stroke
                    float totalLength = 0f;
                    for (int i = 0; i < pointCount - 1; i++)
                    {
                        totalLength += Vector3.Distance(currentBoundaryLine.GetPosition(i), currentBoundaryLine.GetPosition(i + 1));
                    }

                    // 3. THE RATIO LOGIC
                    // We close the loop IF the gap is less than 15% of the total length
                    // AND we add a hard limit (gap must be less than 10cm) so it doesn't snap massive mistakes.
                    if (gapDistance < (totalLength * 0.15f) && gapDistance < 0.10f) 
                    {
                        Debug.Log($"XR SUCCESS: Loop Closed! Gap: {gapDistance:F3}m | Length: {totalLength:F3}m");
                        currentBoundaryLine.SetPosition(pointCount - 1, startPoint);
                    }
                    else
                    {
                        Debug.Log("Left open. User was drawing a detail line.");
                    }
                }
                // Save the finished shape so we can edit it later!
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
}