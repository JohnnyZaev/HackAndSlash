using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
	[RequireComponent(typeof(CharacterController))]
	public class PlayerController : MonoBehaviour
	{
		[SerializeField] private Animator animator;
		private CharacterController _controller;
		private Transform _animatorTransform;

		#region Variables: Movement

		private const float MoveSpeed = 5f;
		private Vector2 _move;
		private Vector3 _moveController;
		private bool _running;

		#endregion

		#region Variables: Input

		private DefaultInputActions _inputActions;
		private InputAction _moveAction;

		#endregion

		#region Variables: Animation

		private int _animRunningParamHash;

		#endregion
		
		private void Awake()
		{
			_inputActions = new DefaultInputActions();
			_controller = GetComponent<CharacterController>();
			_move = new Vector2();
			_moveController = new Vector3();
			_running = false;
			_animRunningParamHash = Animator.StringToHash("Running");
			_animatorTransform = animator.transform;
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
			if (_move.SqrMagnitude() > 0.1f)
			{
				if (!_running)
				{
					_running = true;
					animator.SetBool(_animRunningParamHash, true);
				}
				_moveController.x = _move.x;
				_moveController.z = _move.y;
				_animatorTransform.rotation = Quaternion.LookRotation(-_moveController, Vector3.up);
				_controller.Move(Time.deltaTime * MoveSpeed * _moveController);
			}
			else if (_running)
			{
				_running = false;
				animator.SetBool(_animRunningParamHash, false);
			}
			
		}
	}
}

