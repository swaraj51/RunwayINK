using UnityEngine;
using UnityEngine.UI;

public class VRColorWheel : MonoBehaviour
{
    [Tooltip("The image component displaying the color wheel")]
    [SerializeField] private RectTransform wheelRect;
    
    [Tooltip("The actual image file (MUST have Read/Write enabled in import settings)")]
    [SerializeField] private Texture2D colorWheelTexture;

    private IDrawingEngine drawingEngine;

    private void Start()
    {
        drawingEngine = XRServiceLocator.Resolve<IDrawingEngine>();
    }

    // This triggers when your physical StylusTip pushes into the Canvas
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("StylusTip"))
        {
            PickColorFromTouch(other.transform.position);
        }
    }

    private void PickColorFromTouch(Vector3 stylusWorldPosition)
    {
        // 1. Convert the 3D world position into local 2D Canvas coordinates
        Vector3 localPos = wheelRect.InverseTransformPoint(stylusWorldPosition);

        // 2. Convert those local coordinates into UV space (0.0 to 1.0)
        float u = (localPos.x / wheelRect.rect.width) + 0.5f;
        float v = (localPos.y / wheelRect.rect.height) + 0.5f;

        // 3. Clamp the values so we don't accidentally read off the edge of the image
        u = Mathf.Clamp01(u);
        v = Mathf.Clamp01(v);

        // 4. Read the exact pixel color from the texture!
        Color pickedColor = colorWheelTexture.GetPixelBilinear(u, v);

        // 5. Ignore fully transparent pixels (corners of the circle)
        if (pickedColor.a > 0.1f && drawingEngine != null)
        {
            drawingEngine.SetColor(pickedColor);
            
            // Tiny haptic buzz to confirm you are picking a color
            OVRInput.SetControllerVibration(0.5f, 0.1f, OVRInput.Controller.RTouch); 
        }
    }
}