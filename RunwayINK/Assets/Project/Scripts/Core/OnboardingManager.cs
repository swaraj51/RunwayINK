using System.Collections;
using UnityEngine;
using TMPro;

public class OnboardingManager : MonoBehaviour
{
    [Header("Onboarding Text")]
    public TextMeshProUGUI subtitleText;
    public GameObject subtitlePanel; 

    [Header("Audio Feedback")]
    public AudioSource voiceSource; 
    [Tooltip("Order: 0-Welcome, 1-Colors, 2-Fabrics, 3-Tools, 4-Utilities, 5-Ready")]
    public AudioClip[] onboardingClips;

    [Header("The Invisible Shield")]
    [Tooltip("Drag a transparent 3D Cube here that covers your menu to block the stylus.")]
    public GameObject interactionBlockerShield;

    [Header("UI Elements to Highlight (Drag Transforms here)")]
    public Transform p_Colors;
    public Transform p_Fabrics;
    public Transform p_Tools;
    public Transform utilityButtons; 
    public GameObject mannequin;
    
    [Header("Reveal Animation")]
    public Animator mannequinAnimator;

    private void Start()
    {
        StartCoroutine(RunOnboardingSequence());
    }

    private IEnumerator RunOnboardingSequence()
    {
        // --- PHASE 1: LOCKDOWN ---
        // Ensure the shield is active so they cannot click anything yet!
        if (interactionBlockerShield != null) interactionBlockerShield.SetActive(true);
        mannequin.SetActive(false); // Keep mannequin hidden for the final reveal
        if (subtitlePanel != null) subtitlePanel.SetActive(true);

        // --- PHASE 2: DIRECT ATTENTION ---
        PlayStep(0, "Welcome to RunwayInk. Please look towards your left hand.");
        yield return new WaitForSeconds(6f);

        // --- PHASE 3: THE HIGHLIGHT TOUR ---
        HighlightElement(p_Colors, true);
        PlayStep(1, "This is your Color Palette. Select from a wide range of vibrant dyes.");
        yield return new WaitForSeconds(6f);
        HighlightElement(p_Colors, false);

        HighlightElement(p_Fabrics, true);
        PlayStep(2, "These are your Physical Fabrics. Choose between Silk, Denim, Leather, or Cotton.");
        yield return new WaitForSeconds(6f);
        HighlightElement(p_Fabrics, false);

        HighlightElement(p_Tools, true);
        PlayStep(3, "These are your Brushes. Sketch fine lines, or paint thick ribbons of material.");
        yield return new WaitForSeconds(6f);
        HighlightElement(p_Tools, false);

        HighlightElement(utilityButtons, true);
        PlayStep(4, "Use your utilities to Undo, Clear");
        yield return new WaitForSeconds(6f);
        HighlightElement(utilityButtons, true);

        PlayStep(5, "Your model is ready. You may now start your design journey.");
        subtitleText.text = "";
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
        // --- PHASE 4: UNLOCK AND REVEAL ---
        mannequin.SetActive(true);
        
        // Force the transition from T-Pose to Idle
        if (mannequinAnimator != null)
        {
            mannequinAnimator.enabled = true; 
            // Give it 0.1 seconds to snap from T-pose to the first frame of Idle
            yield return new WaitForSeconds(0.1f); 
            // Freeze her in that Idle pose!
            mannequinAnimator.enabled = false; 
        }

       
        
        // DESTROY THE SHIELD! Buttons are now fully clickable.
        if (interactionBlockerShield != null) interactionBlockerShield.SetActive(false);
        
        yield return new WaitForSeconds(4f);

        // --- PHASE 5: CLEANUP ---
        
    }

    // Helper to keep the code clean and play audio + text together
    private void PlayStep(int index, string text)
    {
        subtitleText.text = text;
        if (voiceSource != null && onboardingClips != null && onboardingClips.Length > index && onboardingClips[index] != null)
        {
            voiceSource.clip = onboardingClips[index];
            voiceSource.Play();
        }
    }

   
    private void HighlightElement(Transform target, bool isHighlighted)
    {
        if (target != null)
        {
            target.gameObject.SetActive(isHighlighted);
            if (isHighlighted) target.localScale = new Vector3(1.02f, 1.02f, 1.02f);
            else target.localScale = new Vector3(1f, 1f, 1f); 
        }
    }
}