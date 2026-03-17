using UnityEngine;

public class DrawingEngine : MonoBehaviour, IDrawingEngine 
{
    [Header("Input Dependencies")]
    [SerializeField] private QuestControllerStylusMock stylusInput;
    
    [Header("Visual Feedback")]
    [SerializeField] private Transform stylusCursor;
    [SerializeField] private float maxCursorSize = 0.02f; 
    
    [Header("Generation (Task 5.3)")]
    [Tooltip("Drag your FabricStroke_Prefab here from the project folder")]
    [SerializeField] private StrokeMeshGenerator strokePrefab; 

    [Header("Surface Projection (Sprint 3)")]
    [Tooltip("The physics layer for the Mannequin")]
    [SerializeField] private LayerMask canvasLayer;
    
    [Tooltip("How far the pen can be before it snaps to the body (in meters)")]
    [SerializeField] private float snapDistance = 0.1f; // 10 cm
    
    [Tooltip("Pushes the fabric off the skin so it doesn't clip inside")]
    [SerializeField] private float skinOffset = 0.005f; // 5 millimeters
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
    private void Update()
    {
        if (stylusInput == null) return;

        UpdateCursorVisuals();
        ProcessInput(stylusInput.TipPosition, stylusInput.TipRotation, stylusInput.Pressure);
    }

    private void UpdateCursorVisuals()
    {
        if (stylusCursor == null) return;
        stylusCursor.position = stylusInput.TipPosition;
        stylusCursor.rotation = stylusInput.TipRotation;
        float currentSize = Mathf.Lerp(0.005f, maxCursorSize, stylusInput.Pressure);
        stylusCursor.localScale = new Vector3(currentSize, currentSize, currentSize);
    }

    public void ProcessInput(Vector3 position, Quaternion rotation, float pressure)
    {
        bool isDrawingNow = stylusInput.IsDrawing;
        
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

        // 1. TRIGGER PULLED & HITTING CANVAS: Start a new stroke
        if (isDrawingNow && !wasDrawingLastFrame && hitCanvas)
        {
            currentStroke = new Stroke();
            currentMeshGenerator = Instantiate(strokePrefab, Vector3.zero, Quaternion.identity);
            currentMeshGenerator.SetFabricColor(currentColor);
        }
        
        // 2. HOLDING TRIGGER & HITTING CANVAS: Add points
        else if (isDrawingNow && wasDrawingLastFrame && hitCanvas)
        {
            // Safety check: Make sure a stroke actually exists (in case they pulled trigger in thin air, then hit the body)
            if (currentStroke == null)
            {
                currentStroke = new Stroke();
                currentMeshGenerator = Instantiate(strokePrefab, Vector3.zero, Quaternion.identity);
                currentMeshGenerator.SetFabricColor(currentColor);
            }

            if (currentStroke.Points.Count == 0 || 
                Vector3.Distance(currentStroke.Points[currentStroke.Points.Count - 1].Position, snappedPosition) > 0.005f)
            {
               currentStroke.AddPoint(new StrokePoint(snappedPosition, rotation, pressure, currentColor, hit.normal));
                currentMeshGenerator.UpdateMesh(currentStroke); 
            }
        }
        
        // 3. TRIGGER RELEASED OR LASER FELL OFF THE BODY: Finish the stroke
        // By checking !hitCanvas here, the stroke automatically cuts off if you slip off the mannequin!
        else if ((!isDrawingNow || !hitCanvas) && wasDrawingLastFrame)
        {
            if (currentStroke != null)
            {
                currentStroke = null;
                currentMeshGenerator = null;
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
}