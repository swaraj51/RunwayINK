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
    
    private Stroke currentStroke;
    private StrokeMeshGenerator currentMeshGenerator;
    private Color currentColor = Color.cyan;
    private bool wasDrawingLastFrame = false;
    private Renderer cursorRenderer;

    private void Start()
    {
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

        // 1. TRIGGER PULLED: Start a new stroke
        if (isDrawingNow && !wasDrawingLastFrame)
        {
            currentStroke = new Stroke();
            
            // Spawn the prefab in the world
            currentMeshGenerator = Instantiate(strokePrefab, Vector3.zero, Quaternion.identity);

            currentMeshGenerator.SetFabricColor(currentColor);
        }
        
        // 2. HOLDING TRIGGER: Add points and update the 3D mesh
        else if (isDrawingNow && wasDrawingLastFrame)
        {
            if (currentStroke.Points.Count == 0 || 
                Vector3.Distance(currentStroke.Points[currentStroke.Points.Count - 1].Position, position) > 0.005f)
            {
                currentStroke.AddPoint(new StrokePoint(position, rotation, pressure, currentColor));
                
                // Tell the Catmull-Rom script to recalculate the fabric shape!
                currentMeshGenerator.UpdateMesh(currentStroke); 
            }
        }
        
        // 3. TRIGGER RELEASED: Finish the stroke
        else if (!isDrawingNow && wasDrawingLastFrame)
        {
            currentStroke = null;
            currentMeshGenerator = null;
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