using UnityEngine;

public class InsertionSlot : MonoBehaviour
{
    // State of the slot
    public LetterBlock OccupyingBlock { get; set; } = null;
    public bool IsOccupied => OccupyingBlock != null;

    private void OnTriggerEnter(Collider other)
    {
        LetterBlock block = other.GetComponent<LetterBlock>();
        if (block != null && block.IsBeingDragged)
        {
            block.currentTargetSlot = this;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        LetterBlock block = other.GetComponent<LetterBlock>();
        if (block != null)
        {
            // Clear intention only if the block is leaving *this* slot
            if (block.currentTargetSlot == this)
            {
                block.currentTargetSlot = null;
            }
        }
    }
}