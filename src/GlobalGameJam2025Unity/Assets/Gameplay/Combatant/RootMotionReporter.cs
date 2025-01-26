using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RootMotionReporter : MonoBehaviour
{
	[SerializeField] private CombatantCharacterController controller;

	private Animator animator;

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	private void OnAnimatorMove()
	{
		controller.transform.position += animator.deltaPosition;

		var eulerDelta = animator.deltaRotation.eulerAngles;

		controller.transform.eulerAngles = new Vector3(
			controller.transform.eulerAngles.x + eulerDelta.x,
			controller.transform.eulerAngles.y + eulerDelta.y,
			controller.transform.eulerAngles.z);
	}
}
