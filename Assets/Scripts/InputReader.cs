using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputReader : MonoBehaviour
{
    PlayerInput playerInput;
    InputAction selectAction;
    InputAction fireAction;

    public event Action Fire;

    public Vector2 Selected => selectAction.ReadValue<Vector2>();

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        selectAction = playerInput.actions["Select"];
        fireAction = playerInput.actions["Fire"];

        fireAction.performed += FireActionOnperformed;
    }

    void OnDestroy()
    {
        fireAction.performed -= FireActionOnperformed;
    }

    private void FireActionOnperformed(InputAction.CallbackContext obj) => Fire?.Invoke();
}