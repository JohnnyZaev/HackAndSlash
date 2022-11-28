using UnityEngine;

namespace Player
{
	[RequireComponent(typeof(CharacterController))]
	public class PlayerController : MonoBehaviour
	{
		private CharacterController _controller;

		#region Variables: Movement

		private const float MoveSpeed = 5f;
		private Vector3 _move;

		#endregion
		private void Start()
		{
			_controller = GetComponent<CharacterController>();
			_move = new Vector3();
		}

		private void Update()
		{
			_move.x = Input.GetAxisRaw("Horizontal");
			_move.z = Input.GetAxisRaw("Vertical");
			_controller.Move(Time.deltaTime * MoveSpeed * _move);
		}
	}
}

