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
                
                // 1. Read the physical nib pressure (pushing against a real desk)
                OVRPlugin.GetActionStateFloat("tip", out float penPressure);
                
                // 2. Read the main index finger button (squeezing in the air)
                OVRPlugin.GetActionStateBoolean("front", out bool frontButton);

                // DEBUG: This will print raw pen data to your console every frame!
                // Debug.Log($"MX Ink -> Nib: {penPressure} | Button: {frontButton}");

                // 3. We draw if the nib is pressed against something OR the front button is held
                bool isDrawing = (penPressure > pressureThreshold) || frontButton;

                if (drawingEngine != null)
                {
                    // Safety check: If they are drawing using the button in the air, 
                    // penPressure is 0. We must force it to 1.0f so the fabric line isn't invisibly thin!
                    float finalPressure = isDrawing && penPressure <= 0.01f ? 1.0f : penPressure;

                    drawingEngine.ProcessInput(penPosition, penRotation, finalPressure, isDrawing);
                }
            }
        }
    }
}