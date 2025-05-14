using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private InputMap inputs = null;

    // Internal input variables
    private Vector2 movementInput = Vector2.zero;
    private Vector2 cameraInput = Vector2.zero;

    // Public accessible input variables
    public Action OnJump; 
    public Vector2 Movement => movementInput;
    public Vector2 Look => cameraInput;

    private void UpdateMovementValue(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.ReadValue<Vector2>();
    }

    private void UpdateCameraValue(InputAction.CallbackContext ctx)
    {
        cameraInput = ctx.ReadValue<Vector2>();
    }

    private void TriggerJumpAction(InputAction.CallbackContext ctx)
    {
        OnJump.Invoke();
    }

    public void CreateInputMap()
    {
        if (inputs != null) return;

        inputs = new();
        inputs.Enable();

        inputs.Player.Move.performed += UpdateMovementValue;
        inputs.Player.Look.performed += UpdateCameraValue;
                
        inputs.Player.Move.canceled += UpdateMovementValue;
        inputs.Player.Look.canceled += UpdateCameraValue;
        
        inputs.Player.Jump.started += TriggerJumpAction;
    }

    private void OnDestroy()
    {
        inputs.Player.Move.performed -= UpdateMovementValue;
        inputs.Player.Look.performed -= UpdateCameraValue;

        inputs.Player.Move.canceled -= UpdateMovementValue;
        inputs.Player.Look.canceled -= UpdateCameraValue;

        inputs.Player.Jump.started -= TriggerJumpAction;
        
        inputs.Disable();
        inputs = null;
    }
}
