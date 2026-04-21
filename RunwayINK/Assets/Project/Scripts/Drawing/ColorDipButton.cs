using UnityEngine;
using UnityEngine.UI;

public class ColorDipButton : MonoBehaviour
{
    [Tooltip("Drag your DrawingEngine here")]
    public DrawingEngine drawingEngine;
    
    [Tooltip("Drag your DockManager here")]
    public DockManager dockManager;

    public void DipStylus()
    {
        // 1. Automatically grab the color from this specific UI button
        Image buttonImage = GetComponent<Image>();
        if (buttonImage != null && drawingEngine != null)
        {
            drawingEngine.SetColor(buttonImage.color);
        }

        // WE DELETED THE FORCED FABRIC SWITCH HERE!
        // Now you stay holding whatever tool you already had equipped.

        Debug.Log("🎨 Dipped Stylus! Color updated.");
    }
}