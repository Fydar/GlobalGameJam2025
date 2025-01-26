using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerCombatantController : MonoBehaviour
{
    private PlayerInput playerInput;
    internal GameManager gameManager;

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
    }

    public void InputOnAttack(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            playerCombatant.Attack();
        }
    }

    public void InputOnMove(InputAction.CallbackContext context)
    {
        var input = context.ReadValue<Vector2>();
        var normalizedInput = input.normalized;
        playerCombatant.inputFilter.Target = normalizedInput;

        if (input.magnitude > 0.01f)
        {
            playerCombatant.rotationDampener.Target = Mathf.Atan2(normalizedInput.x, normalizedInput.y) * Mathf.Rad2Deg;
        }
    }

    public void InputOnLook(InputAction.CallbackContext context)
    {
        var input = context.ReadValue<Vector2>();
        var normalizedInput = input.normalized;

        if (input.magnitude > 0.01f)
        {
            playerCombatant.rotationDampener.Target = Mathf.Atan2(normalizedInput.x, normalizedInput.y) * Mathf.Rad2Deg;
        }
    }

    public void InputOnDash(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            playerCombatant.Dash();
        }
    }
}
