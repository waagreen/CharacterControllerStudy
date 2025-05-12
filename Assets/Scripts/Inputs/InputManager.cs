using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private InputMap inputs;

    // Internal input variables
    private Vector2 movementInput = Vector2.zero;
    private Vector2 cameraInput = Vector2.zero;

    // Public accessible input variables
    public Action<bool> OnJump; 
    public Vector2 Movement => movementInput;
    public Vector2 Look => cameraInput;

    private void UpdateMovementValue(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.ReadValue<Vector2>();
        Vector2.ClampMagnitude(movementInput, 1f);
    }

    private void UpdateCameraValue(InputAction.CallbackContext ctx)
    {
        cameraInput = ctx.ReadValue<Vector2>();
        Vector2.ClampMagnitude(cameraInput, 1f);
    }

    private void TriggerJumpAction(InputAction.CallbackContext ctx)
    {
        OnJump.Invoke(ctx.ReadValue<bool>());
    }

    private void CreateInputMap()
    {
        inputs = new();
        inputs.Enable();

        inputs.Player.Move.performed += UpdateMovementValue;
        inputs.Player.Look.performed += UpdateCameraValue;
        inputs.Player.Jump.started += TriggerJumpAction;
    }

    private void Start()
    {
        CreateInputMap();
    }

    private void OnDestroy()
    {
        inputs.Player.Move.performed -= UpdateMovementValue;
        inputs.Player.Look.performed -= UpdateCameraValue;
        inputs.Player.Jump.started -= TriggerJumpAction;
        
        inputs.Disable();
    }
}
