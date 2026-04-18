using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpatialButtonClicker : MonoBehaviour
{
    [Tooltip("Drag the actions you want to happen here!")]
    public UnityEvent onHoverPoke;
    
    // THE FIX: A cooldown timer to prevent physics jitter "flapping"
    private float cooldownTimer = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Stylus"))
        {
            // If we are still in the cooldown period, ignore the poke!
            if (Time.time < cooldownTimer) return; 
            
            // Set the cooldown for 0.5 seconds from right now
            cooldownTimer = Time.time + 0.5f; 

            Debug.Log($"SPATIAL SUCCESS: Triggered {gameObject.name}");
            onHoverPoke.Invoke(); 
        }
    }
}
