using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {
	/**Object corresponding to player.*/
	[SerializeField] private GameObject m_PlayerGameObj;

	private const string INPUT_AXIS_HORIZONTAL = "Horizontal";
	private const string INPUT_AXIS_VERTICAL = "Vertical";

	public float m_Speed = 1.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		this.MovePlayer ();
	}

	private void MovePlayer()
	{
		Vector3 movement = new Vector3();
		float horizontal_input = Input.GetAxisRaw (INPUT_AXIS_HORIZONTAL);
		float vertical_input = Input.GetAxisRaw (INPUT_AXIS_VERTICAL);

		if (horizontal_input != 0.0f || vertical_input != 0.0f) {
			movement += new Vector3 (horizontal_input * m_Speed, 0.0f, vertical_input * m_Speed);
			if (movement.magnitude > m_Speed) {
				movement = Vector3.Normalize (movement) * m_Speed;
			}
			this.m_PlayerGameObj.GetComponent<PlayerMovement> ().Move (movement);
		}
	}
}
