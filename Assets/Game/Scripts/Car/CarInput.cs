using System;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarInput : MonoBehaviour, GameControls.IVehicleActions
{

    private GameControls _gameControls;

    public float ThrottleInput {get; private set;}
    public float BrakeInput {get; private set;}
    public float SteerInput {get; private set;}

    public event Action OnReversePerformed;
    public event Action OnParkPerformed;
    private void Awake()
    {
        _gameControls = new GameControls();

        _gameControls.Vehicle.SetCallbacks(this);
    }

    private void OnEnable() => _gameControls.Vehicle.Enable();
    private void OnDisable() => _gameControls.Vehicle.Disable();

    public void OnThrottle(InputAction.CallbackContext context) => ThrottleInput = context.ReadValue<float>();
    public void OnBrake(InputAction.CallbackContext context) => BrakeInput = context.ReadValue<float>();
    public void OnSteer(InputAction.CallbackContext context) => SteerInput = context.ReadValue<float>();
    public void OnPark(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnParkPerformed?.Invoke();
        }
    }

    public void OnReverse(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnReversePerformed?.Invoke();
        }
    }

}
