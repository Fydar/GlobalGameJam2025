using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputManager))]
public class GameManager : MonoBehaviour
{
    [SerializeField] internal CinemachineCamera combatCamera;

    [SerializeField] private CinemachineTargetGroup characters;

    [SerializeField] private Animator lobbyFooterAnimator;
    [SerializeField] private Animator[] lobbyChipsAnimators;

    private PlayerInputManager playerInputManager;

    private void Start()
    {
        playerInputManager = GetComponent<PlayerInputManager>();

        characters.Targets.Clear();
    }

    private void Update()
    {
        int readyCount = 1;

        lobbyFooterAnimator.SetBool("IsReady", readyCount == playerInputManager.playerCount);

        for (int i = 0; i < lobbyChipsAnimators.Length; i++)
        {
            var animator = lobbyChipsAnimators[i];
            if (i < readyCount)
            {
                animator.SetInteger("State", 2);
            }
            else if (i < playerInputManager.playerCount)
            {
                animator.SetInteger("State", 1);
            }
            else
            {
                animator.SetInteger("State", 0);
            }
        }
    }

    public void PlayerJoinedEvent(PlayerInput playerInput)
    {
        var playerController = playerInput.GetComponent<PlayerCombatantController>();
        playerController.gameManager = this;
        playerController.playerCombatant.transform.position = characters.transform.position;

        var gamepad = playerInput.GetDevice<Gamepad>();
        if (gamepad != null)
        {
        }

        characters.Targets.Add(new CinemachineTargetGroup.Target()
        {
            Object = playerController.playerCombatant.transform,
            Radius = 1,
            Weight = 1,
        });
    }

    public void PlayerLeftEvent(PlayerInput playerInput)
    {

    }
}
