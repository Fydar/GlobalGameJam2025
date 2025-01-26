using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerCombatantController : MonoBehaviour
{
    private PlayerInput playerInput;
    internal GameManager gameManager;

    private Vector2 moveInput;

    [SerializeField] internal CombatantCharacterController playerCombatant;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (playerInput.currentControlScheme == "Keyboard&Mouse")
        {
            var mainCamera = Camera.main;

            var ray = mainCamera.ScreenPointToRay(Mouse.current.position.value);

            var plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(ray, out float distance))
            {
                var point = ray.GetPoint(distance);
                var planarDistance = new Vector2(point.x, point.z)
                    - new Vector2(playerCombatant.transform.position.x,
                    playerCombatant.transform.position.z);
                var planarVector = planarDistance.normalized;

                playerCombatant.rotationDampener.Target = Mathf.Atan2(planarVector.x, planarVector.y) * Mathf.Rad2Deg;
            }
        }

        var moveInputNormalized = moveInput.normalized;
        var rotatedInput = RotateInput(moveInputNormalized, -gameManager.combatCamera.transform.eulerAngles.y);

        playerCombatant.inputFilter.Target = rotatedInput;

        if (moveInput.magnitude > 0.01f)
        {
            playerCombatant.rotationDampener.Target = (Mathf.Atan2(rotatedInput.x, rotatedInput.y) * Mathf.Rad2Deg);
        }
    }

    public void InputOnAttack(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            playerCombatant.Attack();
        }
    }

    private Vector2 RotateInput(Vector2 vector, float angleDegrees)
    {
        var angle = Mathf.Deg2Rad * angleDegrees;
        float cosAngle = Mathf.Cos(angle);
        float sinAngle = Mathf.Sin(angle);

        return new Vector2(
            (vector.x * cosAngle) - (vector.y * sinAngle),
            (vector.x * sinAngle) + (vector.y * cosAngle)
        );
    }

    public void InputOnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void InputOnLook(InputAction.CallbackContext context)
    {
        var input = context.ReadValue<Vector2>();
        var normalizedInput = input.normalized;

        if (input.magnitude > 0.01f)
        {
            float addRotation = gameManager.combatCamera.transform.eulerAngles.y;

            playerCombatant.rotationDampener.Target = (Mathf.Atan2(normalizedInput.x, normalizedInput.y) * Mathf.Rad2Deg) + addRotation;
        }
    }

    public void InputOnDash(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            if (moveInput.magnitude > 0.01f)
            {
                var moveInputNormalized = moveInput.normalized;
                var rotatedInput = RotateInput(moveInputNormalized, -gameManager.combatCamera.transform.eulerAngles.y);

                var localInput = RotateInput(rotatedInput, playerCombatant.transform.eulerAngles.y);
                playerCombatant.Dash(localInput);
            }
            else
            {
                // If the user isn't pressing a button then dash backwards.
                playerCombatant.Dash(new Vector2(0.0f, -1.0f));
            }
        }
    }
}
