using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
// Note: UnityEngine.XR.Interaction.Toolkit.Interactables is not strictly needed

public class LetterBlock : MonoBehaviour
{
    // --- Data and State ---
    
    public char letterValue = ' ';
    public bool IsBeingDragged { get; private set; } = false; 

    [HideInInspector] public InsertionSlot currentTargetSlot = null; 
    [HideInInspector] public InsertionSlot currentSlot = null; // Track which slot the block is currently in
    [HideInInspector] public bool wasInWord = false; 

    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnBlockGrabbed);
            grabInteractable.selectExited.AddListener(OnBlockReleased);
        }
        else
        {
            Debug.LogError("XR Grab Interactable component not found on LetterBlock! Drag detection will fail.", this);
        }
    }

    // --- Event Handlers ---

    private void OnBlockGrabbed(SelectEnterEventArgs args)
    {
        IsBeingDragged = true;
        
        // If the block is in a slot, notify the manager to clear the slot
        if (WordContainer_Fixed.Instance != null)
        {
            WordContainer_Fixed.Instance.RemoveBlockFromSlot(this);
        }
        
        // Detach from parent (slot)
        transform.SetParent(null);
    }

    private void OnBlockReleased(SelectExitEventArgs args)
    {
        // 1. Reset the drag state
        IsBeingDragged = false;
        
        // 2. Trigger the WordContainer placement logic
        if (WordContainer_Fixed.Instance != null)
        {
            WordContainer_Fixed.Instance.PlaceBlockIntoSlot(this);
        }
    }
    
    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnBlockGrabbed);
            grabInteractable.selectExited.RemoveListener(OnBlockReleased);
        }
    }
}