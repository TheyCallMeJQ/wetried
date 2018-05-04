/*
* A script to be attached as a component to the player gameobject to facilitate movement.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

	/**The marker prefab used to designate where the player is moving to after having clicked.*/
	[SerializeField] private GameObject m_PlayerMovementFlag_Prefab;
	private GameObject m_PlayerMovementFlag_Instance = null;

	private float m_FlagRadius = 1.0f;

	void Update()
	{
		//For use only if we're using mouse input
		this.MovePlayer();
	}

	/**MOUSE-CLICK MOVEMENT:
	 * A function to move the player to the point clicked.
	 * Note: Specifically, this moves the player to the point clicked on the "Floor" layer.
	*/
	public void MovePlayer()
	{
		if (this.m_PlayerMovementFlag_Instance != null) {
			//if the flag exists and the player isn't at the flag, then move towards the flag
			Vector2 player_xz_position = new Vector3(this.transform.position.x, this.transform.position.z);
			Vector2 flag_xz_position = new Vector3 (this.m_PlayerMovementFlag_Instance.transform.position.x,  
				this.m_PlayerMovementFlag_Instance.transform.position.z);
			Vector2 flag_to_player = flag_xz_position - player_xz_position;
			if (flag_to_player.magnitude > this.m_FlagRadius) {
				flag_to_player.Normalize ();
				flag_to_player *= 5.0f * Time.fixedDeltaTime;
				this.transform.position += new Vector3(flag_to_player.x, 0.0f, flag_to_player.y);
			}
			else
			{
				RemovePlayerMovementFlag ();
			}
		}
	}

	/**
	 * A function to create the flag designating where the player gameobject is moving towards*/
	public void CreatePlayerMovementFlag()
	{
		//If there's already a flag when we trigger the flag creation, destroy the currently existing flag to make room for the new one
		if (this.m_PlayerMovementFlag_Instance != null) {
			RemovePlayerMovementFlag ();
		}
		//Create a flag where we clicked
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		foreach (RaycastHit hit in Physics.RaycastAll(ray)) {
			if (hit.collider.gameObject.layer == LayerMask.NameToLayer ("Floor")) {
				Vector3 point_of_impact = hit.point;
				this.m_PlayerMovementFlag_Instance = GameObject.Instantiate (this.m_PlayerMovementFlag_Prefab);
				this.m_PlayerMovementFlag_Instance.transform.position = point_of_impact;
			}
		}
	}

	private void RemovePlayerMovementFlag()
	{
		GameObject.Destroy (this.m_PlayerMovementFlag_Instance);
	}

	/**KEYBOARD MOVEMENT;
	 * A function to be called from the InputManager to translate the gamer input into player motion.
	 * Note: We only actually decide HOW the player moves from the function we call from this function.*/
	public void MovePlayer(float horizontal_input, float vertical_input, float speed)
	{
		if (horizontal_input != 0.0f || vertical_input != 0.0f) {
			Vector3 movement = new Vector3();
			movement += new Vector3 (horizontal_input * speed, 0.0f, vertical_input * speed);
			movement = this.RegulateMagnitude (movement, speed);

			//this is where we choose how to apply the input to the player
//			this.GlobalMove(movement);
			this.LocalMove(movement);
		}
	}//end f'n void MovePlayer(float, float, float)

	/**A function to move the player container object in world coordinates*/
	private void GlobalMove(Vector3 movement)
	{
		this.transform.position += movement * Time.fixedDeltaTime;
	}

	/**A function to move the player container object with respect to where the player is facing (forwards is [this.transform.forward])*/
	private void LocalMove(Vector3 movement)
	{
		Vector3 forward = this.transform.forward * movement.z * Time.fixedDeltaTime;
		Vector3 right = this.transform.right * movement.x * Time.fixedDeltaTime;
		Vector3 final_movement = forward + right;
		this.transform.position += final_movement;
	}



	/**A function to move the player; will rotate the player such that they face where they're moving to, before moving the player.
	*Specifically, will not move while this.transform.forward is not equal to the direction to face.*/
	private void Move_LookBeforeMoving(Vector3 move_by_what, Vector3 direction_to_face)
	{

	}

	/**A helper function to regulate the magnitude of a vector, to ensure the given vector's magnitude never surpasses a given limit.*/
	private Vector3 RegulateMagnitude(Vector3 vector, float limit)
	{
		//if the vector's magnitude surpasses the given limit
		if (vector.magnitude > limit) {
			return (Vector3.Normalize (vector) * limit);
		}
		//else the vector was fine to begin with
		return vector;
	}

}
