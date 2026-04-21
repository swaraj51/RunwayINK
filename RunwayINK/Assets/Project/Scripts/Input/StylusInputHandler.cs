using UnityEngine;

public class StylusInputHandler : MonoBehaviour
{
    [Header("Engine Reference")]
    [SerializeField] private DrawingEngine drawingEngine;

    [Header("Stylus Settings")]
    [SerializeField] private float pressureThreshold = 0.05f;

    private const string MX_INK_PROFILE = "/interaction_profiles/logitech/mx_ink_stylus_logitech";

    void Update()
    {
        string rightDevice = OVRPlugin.GetCurrentInteractionProfileName(OVRPlugin.Hand.HandRight);
        bool isUsingStylus = rightDevice == MX_INK_PROFILE;

        if (isUsingStylus)
        {
            if (OVRPlugin.GetActionStatePose("aim_right", out OVRPlugin.Posef handPose))
            {
                Vector3 penPosition = handPose.Position.FromFlippedZVector3f();
                Quaternion penRotation = handPose.Orientation.FromFlippedZQuatf();
                
                // 1. Read the physical nib (for drawing against a surface)
                OVRPlugin.GetActionStateFloat("tip", out float nibPressure);

                // 2. Read the Grab Button (front index finger) - Reverted back to Drawing!
                OVRPlugin.GetActionStateBoolean("front", out bool grabButton);

                // 3. Read the Middle Rocker switch - We are turning this into our Edit tool!
                OVRPlugin.GetActionStateFloat("middle", out float triggerPressure);

                // --- DRAWING LOGIC ---
                // Draw if the tip is pushed OR the grab button is squeezed
                bool isDrawing = (nibPressure > pressureThreshold) || grabButton;
                float finalPressure = grabButton ? 1.0f : nibPressure;

                if (drawingEngine != null)
                {
                    // Send the Drawing data
                    drawingEngine.ProcessInput(penPosition, penRotation, finalPressure, isDrawing);
        
                    // --- EDITING LOGIC ---
                    // If they squeeze the middle trigger past the threshold, activate Edit Mode!
                    bool isEditing = triggerPressure > pressureThreshold;
                    
                    // Send the Edit data
                    drawingEngine.ProcessEditInput(penPosition, isEditing);
                }
            }
        }
    }
    
}