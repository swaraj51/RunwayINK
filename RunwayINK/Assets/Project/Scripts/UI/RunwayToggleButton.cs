using UnityEngine;
using UnityEngine.UI; // Required for changing UI Images

public class RunwayToggleButton : MonoBehaviour
{
    [Header("Animation Setup")]
    [Tooltip("Drag the Mannequin that has the Animator here")]
    public Animator mannequinAnimator;
    
    [Header("UI Visuals")]
    [Tooltip("Drag the Image component of this button here")]
    public Image buttonImage;
    public Sprite playIcon;
    public Sprite stopIcon;

    private bool isPlaying = false;

    private void Start()
    {
        // Ensure the animator is off at the start so she stands still
        if (mannequinAnimator != null) mannequinAnimator.enabled = false;
        
        // Ensure we start with the Play icon
        if (buttonImage != null && playIcon != null) buttonImage.sprite = playIcon;
    }

    // The Spatial Button will call this method when poked!
    public void OnButtonPoked()
    {
        isPlaying = !isPlaying; // Flip the state (True -> False -> True)

        // 1. Toggle the Animator (Disable it to stop, Enable it to walk)
        if (mannequinAnimator != null)
        {
            mannequinAnimator.enabled = isPlaying;
        }

        // 2. Swap the physical image on the UI button
        if (buttonImage != null)
        {
            buttonImage.sprite = isPlaying ? stopIcon : playIcon;
        }

        Debug.Log("Runway toggled! Is Playing: " + isPlaying);
    }
}