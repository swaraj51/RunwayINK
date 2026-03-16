using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ColorPallet : MonoBehaviour
{[Tooltip("The color this swatch will apply to the drawing engine")]
    [SerializeField] private Color swatchColor = Color.white;
    
    private IDrawingEngine drawingEngine;

    private void Start()
    {
        // 1. Find the XRServiceLocator to get our DrawingEngine safely
        drawingEngine = XRServiceLocator.Resolve<IDrawingEngine>();
        
        // 2. Automatically color this cube's material so you know what color it is
        if (TryGetComponent<Renderer>(out Renderer rend))
        {
            rend.material.color = swatchColor;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 3. When the stylus physically touches this cube, change the color!
        if (other.CompareTag("StylusTip"))
        {
            if (drawingEngine != null)
            {
                drawingEngine.SetColor(swatchColor);
                Debug.Log($"[UI] Color changed to {swatchColor}");
                
                // Optional: Make the controller vibrate slightly when you dip the pen
                OVRInput.SetControllerVibration(1f, 0.5f, OVRInput.Controller.RTouch);
                Invoke(nameof(StopHaptics), 0.1f);
            }
        }
    }

    private void StopHaptics()
    {
        OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
    }
}
