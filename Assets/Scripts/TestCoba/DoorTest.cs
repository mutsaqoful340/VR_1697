using UnityEngine;

public class DoorTest : MonoBehaviour
{
    // The angle in degrees for the door when it is fully open
    [SerializeField] private float openAngle = 90f; 
    // The speed at which the door opens/closes
    [SerializeField] private float rotationSpeed = 2f; 

    private bool isDoorOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        // Store the initial rotation as the closed state
        closedRotation = transform.localRotation;
        // Calculate the open rotation (assuming rotation around Y-axis)
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
    }

    void Update()
    {
        // Determine the target rotation based on the door state
        Quaternion targetRotation = isDoorOpen ? openRotation : closedRotation;
        
        // Smoothly rotate the pivot (and thus the door) towards the target
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation, 
            targetRotation, 
            Time.deltaTime * rotationSpeed
        );
    }

    // Public method to be called by an XR trigger/interactor
    public void ToggleDoor()
    {
        isDoorOpen = !isDoorOpen;
    }
}