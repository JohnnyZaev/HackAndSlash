using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
	[RequireComponent(typeof(CharacterController))]
	public class PlayerController : MonoBehaviour
	{
		private CharacterController _controller;

		#region Variables: Movement

		private const float MoveSpeed = 5f;
		private Vector2 _move;
		private Vector3 _moveController;

		#endregion

		#region Variables: Input

		private DefaultInputActions _inputActions;
		private InputAction _moveAction;

		#endregion
		private void Awake()
		{
			_inputActions = new DefaultInputActions();
			_controller = GetComponent<CharacterController>();
			_move = new Vector2();
			_moveController = new Vector3();
		}

		private void OnEnable()
		{
			_moveAction = _inputActions.Player.Move;
			_moveAction.Enable();
		}

		private void OnDisable()
		{
			_moveAction.Disable();
		}

		private void Update()
		{
			_move = _moveAction.ReadValue<Vector2>();
			_moveController.x = _move.x;
			_moveController.z = _move.y;
			_controller.Move(Time.deltaTime * MoveSpeed * _moveController);
		}
	}
}

