using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class SpatialButton : MonoBehaviour
{
    public UnityEvent onHoverPoke;
    
    [Header("Tooltip UX")]
    [Tooltip("Create a 3D TextMeshPro object above the button and drag it here")]
    public GameObject hoverTooltipText; 
    
    private float cooldownTimer = 0f;

    void Start()
    {
        // Hide the tooltip when the app starts
        if (hoverTooltipText != null) hoverTooltipText.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Stylus"))
        {
            // Show the text when the pen hovers near the button!
            if (hoverTooltipText != null) hoverTooltipText.SetActive(true);

            if (Time.time < cooldownTimer) return; 
            cooldownTimer = Time.time + 0.5f; 
            
            onHoverPoke.Invoke(); 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Stylus"))
        {
            // Hide the text when the pen leaves
            if (hoverTooltipText != null) hoverTooltipText.SetActive(false);
        }
    }
}