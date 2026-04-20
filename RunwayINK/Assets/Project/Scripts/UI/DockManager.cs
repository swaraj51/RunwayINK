using UnityEngine;

public class DockManager : MonoBehaviour
{
    [Header("Assign your 'P_' Panels here")]
    public GameObject p_Colors;
    public GameObject p_Fabrics;
    public GameObject p_Tools;
    
    [Header("Engine Reference")]
    public DrawingEngine drawingEngine;

    // --- NEW: THE MISSING FABRIC VARIABLES ---
    [Header("Fabric Materials")]
    public Material silkMaterial;
    public Material denimMaterial;
    public Material leatherMaterial;
    [Header("Cursor Visuals")]
    [Tooltip("Drag the sphere on the tip of your stylus here")]
    public Renderer stylusCursorRenderer;

    void Start()
    {
        // Start with a clean view: close everything!
        CloseAllPanels();
    }

    public void ToggleColors()
    {
        bool isCurrentlyOpen = p_Colors.activeSelf;
        CloseAllPanels(); 
        if (!isCurrentlyOpen) p_Colors.SetActive(true); 
    }

    public void ToggleFabrics()
    {
        bool isCurrentlyOpen = p_Fabrics.activeSelf;
        CloseAllPanels();
        if (!isCurrentlyOpen) p_Fabrics.SetActive(true);
    }

    public void ToggleTools()
    {
        bool isCurrentlyOpen = p_Tools.activeSelf;
        CloseAllPanels();
        if (!isCurrentlyOpen) p_Tools.SetActive(true);
    }

    private void CloseAllPanels()
    {
        if (p_Colors != null) p_Colors.SetActive(false);
        if (p_Fabrics != null) p_Fabrics.SetActive(false);
        if (p_Tools != null) p_Tools.SetActive(false);
    }
   
    public void ActivateSketchPencil()
    {
        if (drawingEngine != null)
        {
            drawingEngine.isDrapeMode = false;
            // WE REMOVED THE FORCED WHITE COLOR HERE!
            
            // Visual Update: Shrink the cursor back to pencil size, but keep its current color!
            if(stylusCursorRenderer != null) {
                stylusCursorRenderer.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
            }
            // CloseAllPanels(); // (Leave commented out if you like the menu staying open)
        }
    }

    public void ActivateFabricMarker()
    {
        if (drawingEngine != null)
        {
            drawingEngine.isDrapeMode = true;
            
            if(stylusCursorRenderer != null) {
                stylusCursorRenderer.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
            }
            // CloseAllPanels();  <-- DELETE OR COMMENT OUT THIS LINE
        }
    }

    // --- NEW: THE MISSING FABRIC SWITCHING METHODS ---
    public void SelectFabricSilk()
    {
        if (drawingEngine != null) { drawingEngine.currentFabricMaterial = silkMaterial; ActivateFabricMarker(); }
    }
    
    public void SelectFabricDenim()
    {
        if (drawingEngine != null) { drawingEngine.currentFabricMaterial = denimMaterial; ActivateFabricMarker(); }
    }
    
    public void SelectFabricLeather()
    {
        if (drawingEngine != null) { drawingEngine.currentFabricMaterial = leatherMaterial; ActivateFabricMarker(); }
    }
}