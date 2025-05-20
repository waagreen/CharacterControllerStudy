using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private InputMap inputs = null;

    // Internal input variables
    private Vector2 movementInput = Vector2.zero;
    private Vector2 cameraInput = Vector2.zero;
    private bool jumpValue = false;

    // Public accessible input variables
    public Action<bool> OnJump; 
    public Vector2 Movement { get => movementInput; }
    public Vector2 Look { get => cameraInput; }
    public bool JumpValue { get => jumpValue; }
    public InputAction Jump { get => inputs.Player.Jump; }

    private void UpdateMovementValue(InputAction.CallbackContext ctx)
    {
        movementInput = Vector3.ClampMagnitude(ctx.ReadValue<Vector2>(), 1f);

    }

    private void UpdateCameraValue(InputAction.CallbackContext ctx)
    {
        cameraInput = Vector3.ClampMagnitude(ctx.ReadValue<Vector2>(), 1f);
    }

    private void TriggerJumpAction(InputAction.CallbackContext ctx)
    {
        jumpValue = ctx.ReadValueAsButton();
        OnJump?.Invoke(jumpValue);
    }

    public void CreateInputMap()
    {
        if (inputs != null) return;

        inputs = new();
        inputs.Enable();

        inputs.Player.Look.performed += UpdateCameraValue;
        inputs.Player.Look.started += UpdateCameraValue;
        inputs.Player.Look.canceled += UpdateCameraValue;

        inputs.Player.Move.performed += UpdateMovementValue;
        inputs.Player.Move.canceled += UpdateMovementValue;
        inputs.Player.Move.started += UpdateMovementValue;

        inputs.Player.Jump.started += TriggerJumpAction;
        inputs.Player.Jump.canceled += TriggerJumpAction;
    }

    private void OnDestroy()
    {
        inputs.Player.Look.performed -= UpdateCameraValue;
        inputs.Player.Look.started -= UpdateCameraValue;
        inputs.Player.Look.canceled -= UpdateCameraValue;

        inputs.Player.Move.performed -= UpdateMovementValue;
        inputs.Player.Move.canceled -= UpdateMovementValue;
        inputs.Player.Move.started -= UpdateMovementValue;

        inputs.Player.Jump.started -= TriggerJumpAction;
        inputs.Player.Jump.canceled -= TriggerJumpAction;
        
        inputs.Disable();
        inputs = null;
    }
}
