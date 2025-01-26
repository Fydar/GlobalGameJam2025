using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class HapticsAnimator : MonoBehaviour, ISerializationCallbackReceiver
{
    [Range(0.0f, 1.0f)]
    [SerializeField] private float lowFrequencyMotorSpeed;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float highFrequencyMotorSpeed;

    private PlayerInput playerInput;

    private void Update()
    {
        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
        }
        var gamepad = playerInput.GetDevice<Gamepad>();
        gamepad?.SetMotorSpeeds(lowFrequencyMotorSpeed, highFrequencyMotorSpeed);
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
    }
}
