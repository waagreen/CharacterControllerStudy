using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private InputMap inputs = null;

    // Internal input variables
    private float diveValue = 0f;

    private Vector2 movementInput = Vector2.zero;
    private Vector2 cameraInput = Vector2.zero;

    private bool jumpValue = false;
    private bool climbValue = false;

    // Public accessible input variables
    public InputAction Jump => inputs.Player.Jump;

    public float Dive => diveValue;
    public Vector2 Movement => movementInput;
    public Vector2 Look => cameraInput;

    public bool JumpValue => jumpValue;
    public bool ClimbValue => climbValue;

    private void UpdateMovementInput(InputAction.CallbackContext ctx)
    {
        movementInput = Vector3.ClampMagnitude(ctx.ReadValue<Vector2>(), 1f);
    }

    private void UpdateCameraInput(InputAction.CallbackContext ctx)
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

    private void UpdateDiveValue(InputAction.CallbackContext ctx)
    {
        diveValue = ctx.ReadValue<float>();
    }

    public void CreateInputMap()
    {
        if (inputs != null) return;

        inputs = new();
        inputs.Enable();

        inputs.Player.Look.performed += UpdateCameraInput;
        inputs.Player.Look.started += UpdateCameraInput;
        inputs.Player.Look.canceled += UpdateCameraInput;

        inputs.Player.Move.performed += UpdateMovementInput;
        inputs.Player.Move.canceled += UpdateMovementInput;
        inputs.Player.Move.started += UpdateMovementInput;

        inputs.Player.Jump.started += UpdateJumpValue;
        inputs.Player.Jump.canceled += UpdateJumpValue;

        inputs.Player.Climb.started += UpdateClimbValue;
        inputs.Player.Climb.performed += UpdateClimbValue;
        inputs.Player.Climb.canceled += UpdateClimbValue;

        inputs.Player.Dive.started += UpdateDiveValue;
        inputs.Player.Dive.performed += UpdateDiveValue;
        inputs.Player.Dive.canceled += UpdateDiveValue;
    }

    private void OnDestroy()
    {
        inputs.Player.Look.performed -= UpdateCameraInput;
        inputs.Player.Look.started -= UpdateCameraInput;
        inputs.Player.Look.canceled -= UpdateCameraInput;

        inputs.Player.Move.performed -= UpdateMovementInput;
        inputs.Player.Move.canceled -= UpdateMovementInput;
        inputs.Player.Move.started -= UpdateMovementInput;

        inputs.Player.Jump.started -= UpdateJumpValue;
        inputs.Player.Jump.canceled -= UpdateJumpValue;

        inputs.Player.Climb.started -= UpdateClimbValue;
        inputs.Player.Climb.performed -= UpdateClimbValue;
        inputs.Player.Climb.canceled -= UpdateClimbValue;
        
        inputs.Player.Dive.started += UpdateDiveValue;
        inputs.Player.Dive.performed += UpdateDiveValue;
        inputs.Player.Dive.canceled += UpdateDiveValue;

        inputs.Disable();
        inputs = null;
    }
}
