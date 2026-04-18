using UnityEngine;

public class DockManager : MonoBehaviour
{
    [Header("Assign your 'P_' Panels here")]
    public GameObject p_Colors;
    public GameObject p_Fabrics;
    public GameObject p_Tools;
    [Header("Engine Reference")]
    public DrawingEngine drawingEngine;

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
   
    // --- TOOL SWITCHING METHODS ---
    public void ActivateSketchPencil()
    {
        if (drawingEngine != null)
        {
            drawingEngine.isDrapeMode = false;
            drawingEngine.SetColor(Color.white); // Force the pencil to be white!
            Debug.Log("Equipped: White Structural Pencil");
            CloseAllPanels(); // Auto-close the menu so they can get right to drawing
        }
    }

    public void ActivateFabricMarker()
    {
        if (drawingEngine != null)
        {
            drawingEngine.isDrapeMode = true;
            Debug.Log("Equipped: 3D Fabric Marker");
            CloseAllPanels(); // Auto-close the menu
        }
    }
}