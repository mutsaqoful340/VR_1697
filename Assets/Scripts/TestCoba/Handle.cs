using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Handle : MonoBehaviour
{
    [Header("Handle Anchor")]
    public GameObject HandleAnchor;
    
    private Rigidbody rb;
    public enum HandleStates
    {
        Inactive,
        Active
    }
    
    public HandleStates HandleState;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        HandleState = HandleStates.Inactive;
    }

    public void Handle_Grabbed()
    {
        // Mark as active when grabbed and allow free movement
        HandleState = HandleStates.Active;
        Debug.Log(gameObject.name + " grabbed. State: " + HandleState.ToString());
    }

    public void Handle_Released()
    {
        // Mark as inactive when released and snap back to anchor
        HandleState = HandleStates.Inactive;
        Debug.Log(gameObject.name + " released. State: " + HandleState.ToString());
    }

    private void Follow_Anchor()
    {
        if (HandleAnchor != null && HandleState == HandleStates.Inactive)
        {
            this.transform.position = HandleAnchor.transform.position;
            this.transform.rotation = HandleAnchor.transform.rotation;
        }
        else if (HandleState == HandleStates.Active)
        {
            // Do nothing, allow free movement
        }
    }

    private void Update()
    {
        if (HandleAnchor != null)
        {
            Follow_Anchor();
        }
    }
}
