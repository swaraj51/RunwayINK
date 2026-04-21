using UnityEngine;
using UnityEngine.UI;

public class PaletteManager : MonoBehaviour
{
    [Header("Engine References")]
    [Tooltip("Drag your DrawingEngine here so we can send the color to it")]
    public DrawingEngine drawingEngine;
    
    [Tooltip("Drag the GameObject that holds your 56 Images (e.g., Panel_Colors) here")]
    public GameObject colorGridPanel;

    // 56 Curated Fashion Colors (8 Columns x 7 Rows)
    private readonly string[] hexColors = new string[56] 
    {
        // Row 1: Grayscale
        "#FFFFFF", "#E0E0E0", "#C0C0C0", "#A0A0A0", "#808080", "#606060", "#404040", "#000000",
        // Row 2: Reds & Pinks
        "#FFEBEE", "#FFCDD2", "#EF9A9A", "#E57373", "#EF5350", "#F44336", "#E53935", "#D32F2F",
        // Row 3: Oranges & Peaches
        "#FFF3E0", "#FFE0B2", "#FFCC80", "#FFB74D", "#FFA726", "#FF9800", "#FB8C00", "#F57C00",
        // Row 4: Yellows & Golds
        "#FFFDE7", "#FFF9C4", "#FFF59D", "#FFF176", "#FFEE58", "#FFEB3B", "#FDD835", "#FBC02D",
        // Row 5: Greens
        "#E8F5E9", "#C8E6C9", "#A5D6A7", "#81C784", "#66BB6A", "#4CAF50", "#43A047", "#388E3C",
        // Row 6: Blues & Cyans
        "#E3F2FD", "#BBDEFB", "#90CAF9", "#64B5F6", "#42A5F5", "#2196F3", "#1E88E5", "#1976D2",
        // Row 7: Purples & Leathers (Browns)
        "#F3E5F5", "#E1BEE7", "#CE93D8", "#BA68C8", "#8D6E63", "#795548", "#5D4037", "#4E342E"
    };

    void Start()
    {
        if (colorGridPanel == null)
        {
            Debug.LogError("PaletteManager: Please assign the Color Grid Panel in the Inspector!");
            return;
        }

        // Automatically find all Image components inside your grid
        Image[] colorSquares = colorGridPanel.GetComponentsInChildren<Image>();
        int colorIndex = 0;

        foreach (Image square in colorSquares)
        {
            // Skip the parent panel itself if it happens to have an Image component
            if (square.gameObject == colorGridPanel) continue;

            // Stop if we run out of colors
            if (colorIndex >= hexColors.Length) break;

            // 1. Convert the Hex Code to a Unity Color
            if (ColorUtility.TryParseHtmlString(hexColors[colorIndex], out Color newColor))
            {
                // 2. Paint the white square
                square.color = newColor;

                // 3. Automatically add a Button component if you haven't already
                Button btn = square.GetComponent<Button>();
                if (btn == null)
                {
                    btn = square.gameObject.AddComponent<Button>();
                }

                // 4. Wire up the "Dip" click event
                btn.onClick.AddListener(() => OnColorSelected(newColor));
            }
            colorIndex++;
        }
        
        Debug.Log($"PaletteManager: Successfully generated {colorIndex} colors!");
    }

    // This is triggered whenever your stylus clicks a color square
    // This is triggered whenever your stylus clicks a color square
    private void OnColorSelected(Color selectedColor)
    {
        if (drawingEngine != null)
        {
            // USE THE EXISTING METHOD TO SET THE COLOR!
            drawingEngine.SetColor(selectedColor);
            
            // Auto-switch to the Fabric Marker mode so they can draw immediately!
            drawingEngine.isDrapeMode = true; 
            
            Debug.Log($"Switched to Fabric Tool! Color set to: {selectedColor}");
        }
    }
}