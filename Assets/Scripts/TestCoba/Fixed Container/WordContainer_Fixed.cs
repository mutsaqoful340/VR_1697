using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
// Note: UnityEngine.XR.Interaction.Toolkit.Interactables is not strictly needed here

public class WordContainer_Fixed : MonoBehaviour
{
    // --- Configuration (Minimal) ---
    public static WordContainer_Fixed Instance { get; private set; }

    [Header("Game Dictionary")]
    // The list of valid words, editable in the Inspector
    public List<string> validWords = new List<string> {}; 

    [Header("Animation Settings")]
    public Animator WordContainerAnim;
    
    // List of all fixed slots in order (set up in the editor)
    private List<InsertionSlot> allSlots;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            WordContainerAnim.SetTrigger("PlayerIN");
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            WordContainerAnim.SetTrigger("PlayerOUT");
        }
    }

    void Awake()
    {
        Instance = this;
        // Collect all fixed InsertionSlot children in order on Awake, ordering by X position.
        allSlots = GetComponentsInChildren<InsertionSlot>().OrderBy(s => s.transform.localPosition.x).ToList();
    }

    // Called by LetterBlock.cs when the block is released
    public void PlaceBlockIntoSlot(LetterBlock block)
    {
        // 1. Get the target slot from the LetterBlock's state
        InsertionSlot targetSlot = block.currentTargetSlot;
        
        // Validation: Must have a target AND the slot must be empty
        if (targetSlot == null || targetSlot.IsOccupied)
        {
            block.currentTargetSlot = null;
            return; 
        }

        // 2. Finalize Placement
        block.transform.SetParent(targetSlot.transform);
        block.transform.localPosition = Vector3.zero; // Snap to the center of the slot
        block.transform.localRotation = Quaternion.identity;

        // Update Slot State
        targetSlot.OccupyingBlock = block;
        block.wasInWord = true;
        block.currentSlot = targetSlot; // Track which slot the block is in
        block.currentTargetSlot = null; // Clear the target state

        // 3. Check Completion
        CheckWordCompletion();
    }

    // Called by LetterBlock.cs when the block is grabbed/removed from a slot
    public void RemoveBlockFromSlot(LetterBlock block)
    {
        if (block.currentSlot != null)
        {
            // Clear the slot's occupancy
            block.currentSlot.OccupyingBlock = null;
            block.currentSlot = null;
            
            // Optionally: Re-check if word is still valid after removal
            CheckWordCompletion();
        }
    }

    public void CheckWordCompletion()
    {
        IEnumerable<InsertionSlot> occupiedSlots = allSlots
            .Where(s => s.IsOccupied);
        
        if (!occupiedSlots.Any()) return;
        
        // 1. Assemble the word string
        string currentWord = "";
        foreach (InsertionSlot slot in occupiedSlots)
        {
            currentWord += slot.OccupyingBlock.letterValue;
        }
        
        currentWord = currentWord.ToUpper();

        // 2. Check the dictionary
        if (IsWordValid(currentWord))
        {
            Debug.Log($"SUCCESS! Valid word formed: {currentWord}");
            HandleValidWord(currentWord);
        }
    }

    private void HandleValidWord(string word)
    {
        IEnumerable<LetterBlock> wordBlocks = allSlots
            .Where(s => s.IsOccupied)
            .Select(s => s.OccupyingBlock);

        // Action 1: Lock all LetterBlocks in place
        foreach (LetterBlock block in wordBlocks)
        {
            XRGrabInteractable grabInteractable = block.GetComponent<XRGrabInteractable>();
            if (grabInteractable != null)
            {
                // Temporarily disable grabbing so the word cannot be dismantled
                grabInteractable.enabled = false; 
            }
        }
        // Action 2: Trigger celebration, next level load, etc.
    }

    private bool IsWordValid(string word)
    {
        // Check the word against the list provided in the Inspector
        return validWords.Contains(word);
    }
}