using UnityEngine;
using UnityEngine.AI;

public class CombatantCharacterController : MonoBehaviour
{
    [SerializeField] private Animator graphicsAnimator;
    [SerializeField] private AnimatorFloatLabel_Halting graphicsHalting;
    [SerializeField] private AnimatorFloatLabel_Halting graphicsZeroing;

    [Header("Moving")]
    [SerializeField] private float walkSpeed;
    [SerializeReference] internal IInterpolatorVector2 inputFilter = new InterpolatorVector2PerAxis(
        new SpringInterpolator(8.0f, 0.2f, 0.0f),
        new SpringInterpolator(8.0f, 0.2f, 0.0f));

    [Header("Turning")]
    [SerializeField] private float rotationSpeed;
    [SerializeField]
    internal SmoothDampAngleInterpolator rotationDampener = new(0.1f, 60.0f);

    public void Update()
    {
        inputFilter.Update(Time.smoothDeltaTime);
        rotationDampener.Update(Time.smoothDeltaTime * graphicsZeroing.Value);

        var filteredInput = inputFilter.Value;

        transform.position += walkSpeed * Time.deltaTime * graphicsHalting.Value * new Vector3(filteredInput.x, 0.0f, filteredInput.y);

        transform.rotation = Quaternion.Euler(0.0f, rotationDampener.Value, 0.0f);

        graphicsAnimator.SetFloat("LocomotionY", filteredInput.y);
        graphicsAnimator.SetFloat("LocomotionX", filteredInput.x);

        // Clamp the position to a point on the NavMesh.
        if (NavMesh.SamplePosition(transform.position, out var hit, 3.0f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
        }
    }
    //  playerCombatant.transform.LookAt(transform.position + new Vector3(movement.x, 0.0f, movement.y));

    public void Attack()
    {
        graphicsAnimator.SetTrigger("Attack");
    }

    public void Dash(Vector2 direction)
    {
        // var localDash = Quaternion.Inverse(transform.rotation) * new Vector3(target.x, 0.0f, target.y);

        graphicsAnimator.SetTrigger("Dash");
        graphicsAnimator.SetFloat("DashX", direction.x);
        graphicsAnimator.SetFloat("DashY", direction.y);
    }
}
