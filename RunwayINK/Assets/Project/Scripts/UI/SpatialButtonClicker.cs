using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class SpatialButton : MonoBehaviour
{
    public UnityEvent onHoverPoke;
    
    [Header("Audio Feedback")]
    public AudioSource buttonAudioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;
    
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
        // ONLY trigger if the Stylus touches it
        if (other.CompareTag("Stylus"))
        {
            // 1. Show the Tooltip
            if (hoverTooltipText != null) hoverTooltipText.SetActive(true);

            // 2. Play the Hover/Tick sound just for touching the button
            if (buttonAudioSource != null && hoverSound != null) 
            {
                buttonAudioSource.PlayOneShot(hoverSound);
            }

            // 3. Cooldown check (so it doesn't double-click!)
            if (Time.time < cooldownTimer) return; 
            cooldownTimer = Time.time + 0.5f; 
            
            // 4. Play the satisfying Click/Pop sound right as the action happens
            if (buttonAudioSource != null && clickSound != null) 
            {
                buttonAudioSource.PlayOneShot(clickSound);
            }

            // 5. Fire the actual button event
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