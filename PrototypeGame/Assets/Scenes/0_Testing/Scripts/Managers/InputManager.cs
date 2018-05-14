using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {
	/**Object corresponding to player container.*/
	[SerializeField] private GameObject m_PlayerContainer;

	private const string INPUT_AXIS_HORIZONTAL = "Horizontal";
	private const string INPUT_AXIS_VERTICAL = "Vertical";

	/**The button we'll use for movement (I'm not sure which button we want to use, so I'll set it to a default and facilitate testing for later)*/
	public int m_MouseButton = 0;

	// Update is called once per frame
	void Update () {
//		this.ProcessPlayerMovementInput_Keyboard();
		this.ProcessPlayerMovementInput_MouseClick();
	}

//	/**A function to take care of processing player movement input, where the classic WASD/arrow keys control movement.*/
//	private void ProcessPlayerMovementInput_Keyboard()
//	{
//		float horizontal_input = Input.GetAxisRaw (INPUT_AXIS_HORIZONTAL);
//		float vertical_input = Input.GetAxisRaw (INPUT_AXIS_VERTICAL);
//		this.m_PlayerContainer.GetComponent<PlayerMovement> ().SetMovementFromInput (horizontal_input, vertical_input);
//	}

	/**A function to take care of processing player movement input, where the player moves to the point clicked*/
	private void ProcessPlayerMovementInput_MouseClick()
	{
		if (Input.GetMouseButtonDown (this.m_MouseButton)) {
			this.m_PlayerContainer.GetComponent<PlayerMovement> ().CreatePlayerMovementFlag();
		}
	}

}
