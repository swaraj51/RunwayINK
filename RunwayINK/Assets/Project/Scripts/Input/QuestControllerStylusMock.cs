using System.Collections.Generic;
using UnityEngine;

// Task 4.1 & 4.5: Stylus Input Handler with Jitter Reduction
public class QuestControllerStylusMock : MonoBehaviour, IStylusProvider
{
    [Header("Hardware Settings")]
    [Tooltip("Which controller acts as the stylus?")]
    [SerializeField] private OVRInput.Controller controller = OVRInput.Controller.RTouch;
    
    [Tooltip("Create an empty GameObject at the tip of your controller model and assign it here")]
    [SerializeField] private Transform controllerTipTransform; 
    
    [Header("Smoothing Settings")]
    [Tooltip("Higher = smoother strokes but more input lag. 3-5 is ideal for Quest controllers.")]
    [SerializeField, Range(1, 10)] private int smoothingFrames = 4;
    
    private Queue<Vector3> positionBuffer = new Queue<Vector3>();

    public Vector3 TipPosition { get; private set; }
    public Quaternion TipRotation => controllerTipTransform != null ? controllerTipTransform.rotation : transform.rotation;
    
    // Map the analog index trigger to simulate stylus pressure (0.0 to 1.0)
    public float Pressure => OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controller);
    
    public bool IsDrawing => Pressure > 0.1f;
    
    public bool IsEraser => OVRInput.Get(OVRInput.Button.One, controller); 

    private void Update()
    {
        if (controllerTipTransform == null) return;
        UpdateSmoothedPosition(controllerTipTransform.position);
    }

    private void UpdateSmoothedPosition(Vector3 rawPosition)
    {
        positionBuffer.Enqueue(rawPosition);
        
        if (positionBuffer.Count > smoothingFrames)
        {
            positionBuffer.Dequeue();
        }

        Vector3 sum = Vector3.zero;
        foreach (var pos in positionBuffer)
        {
            sum += pos;
        }
        TipPosition = sum / positionBuffer.Count;
    }
}