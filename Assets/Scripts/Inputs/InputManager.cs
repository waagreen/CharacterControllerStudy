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
    private bool climbValue = false;

    // Public accessible input variables
    public Vector2 Movement => movementInput;
    public Vector2 Look => cameraInput;
    public InputAction Jump => inputs.Player.Jump;
    public bool JumpValue  => jumpValue;
    public bool ClimbValue => climbValue;

    private void UpdateMovementValue(InputAction.CallbackContext ctx)
    {
        movementInput = Vector3.ClampMagnitude(ctx.ReadValue<Vector2>(), 1f);
    }

    private void UpdateCameraValue(InputAction.CallbackContext ctx)
    {
        cameraInput = Vector3.ClampMagnitude(ctx.ReadValue<Vector2>(), 1f);
    }

    private void UpdateJumpValue(InputAction.CallbackContext ctx)
    {
        jumpValue = ctx.ReadValueAsButton();
    }

    private void UpdateClimbValue(InputAction.CallbackContext ctx)
    {
        climbValue = ctx.ReadValueAsButton();
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

        inputs.Player.Jump.started += UpdateJumpValue;
        inputs.Player.Jump.canceled += UpdateJumpValue;
        
        inputs.Player.Climb.started += UpdateClimbValue;
        inputs.Player.Climb.performed += UpdateClimbValue;
        inputs.Player.Climb.canceled += UpdateClimbValue;
    }

    private void OnDestroy()
    {
        inputs.Player.Look.performed -= UpdateCameraValue;
        inputs.Player.Look.started -= UpdateCameraValue;
        inputs.Player.Look.canceled -= UpdateCameraValue;

        inputs.Player.Move.performed -= UpdateMovementValue;
        inputs.Player.Move.canceled -= UpdateMovementValue;
        inputs.Player.Move.started -= UpdateMovementValue;

        inputs.Player.Jump.started -= UpdateJumpValue;
        inputs.Player.Jump.canceled -= UpdateJumpValue;

        inputs.Player.Climb.started -= UpdateClimbValue;
        inputs.Player.Climb.performed -= UpdateClimbValue;
        inputs.Player.Climb.canceled -= UpdateClimbValue;
        
        inputs.Disable();
        inputs = null;
    }
}
