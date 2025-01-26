using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerController : MonoBehaviour
{
    private PlayerInput playerInput;
    private Animator playerAnimator;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerAnimator = GetComponent<Animator>();

        playerAnimator.SetTrigger("Join");
    }

    public void InputOnUnjoin(InputAction.CallbackContext context)
    {
        if (context.duration > 0.4)
        {
            playerAnimator.SetTrigger("Unjoin");
        }
    }

    public void EventOnFinishUnjoin()
    {
        Destroy(gameObject);
    }
}
