/*
* A script to be attached as a component to the player container gameobject to facilitate movement.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

	/**The marker prefab used to designate where the player is moving to after having clicked.*/
	[SerializeField] private GameObject m_PlayerMovementFlag_Prefab;
	private GameObject m_PlayerMovementFlag_Instance = null;

	/**The movement vector being applied to the player gameobject, defined in global coordinates*/
	private Vector3 m_Movement = Vector3.zero;

	/**The speed at which the player moves*/
	public float m_Speed;

	private Animator m_PlayerMovementAnimator;

	private const string ANIMATOR_PARAM_ISMOVING_LEFT = "isMovingLeft";
	private const string ANIMATOR_PARAM_ISMOVING_RIGHT = "isMovingRight";
	private const string ANIMATOR_PARAM_ISMOVING_UP = "isMovingUp";
	private const string ANIMATOR_PARAM_ISMOVING_DOWN = "isMovingDown";

	[SerializeField] private Camera m_Camera;

	void Awake()
	{
		this.m_PlayerMovementAnimator = this.GetComponentInChildren<Animator> ();
	}

	void Update()
	{
		this.ApplyMovement();
		this.UpdateAnimator ();
	}

	/**A function to actually apply the movement to the player gameobject, context-depending.*/
	private void ApplyMovement()
	{
		//If the flag exists (this implies that the player just clicked to initiate movement)
		if (this.m_PlayerMovementFlag_Instance != null) {
			//...then move the player to the flag

			//if the player isn't at the flag, then move towards the flag
			Vector2 player_xz_position = new Vector3(this.transform.position.x, this.transform.position.z);
			Vector2 flag_xz_position = new Vector3 (this.m_PlayerMovementFlag_Instance.transform.position.x,  
				this.m_PlayerMovementFlag_Instance.transform.position.z);
			Vector2 flag_to_player = flag_xz_position - player_xz_position;

			if (flag_to_player.magnitude > this.m_PlayerMovementFlag_Prefab.GetComponent<SphereCollider>().radius) {
				flag_to_player.Normalize ();
				flag_to_player *= this.m_Speed * Time.fixedDeltaTime;
				//				this.transform.position += new Vector3(flag_to_player.x, 0.0f, flag_to_player.y);
				this.m_Movement = new Vector3(flag_to_player.x, 0.0f, flag_to_player.y);
			}
			else
			{
				RemovePlayerMovementFlag ();
			}
		}//end if
		//and if the movement instance variable is n/e to zero, then move us towards where we need to be
		if (this.m_Movement != Vector3.zero) {
			this.transform.position += this.m_Movement;
		}
	}

	/**A function to update the player animator, with respect to the movement input (specifically, with respect to [this.m_Movement]*/
	private void UpdateAnimator()
	{
		this.m_PlayerMovementAnimator.SetBool (ANIMATOR_PARAM_ISMOVING_LEFT, this.m_Movement.x < 0.0f);
		this.m_PlayerMovementAnimator.SetBool (ANIMATOR_PARAM_ISMOVING_RIGHT, this.m_Movement.x > 0.0f);
		this.m_PlayerMovementAnimator.SetBool (ANIMATOR_PARAM_ISMOVING_UP, this.m_Movement.z > 0.0f);
		this.m_PlayerMovementAnimator.SetBool (ANIMATOR_PARAM_ISMOVING_DOWN, this.m_Movement.z < 0.0f);
		//Note: Idle behavior is defined as an absence of any motion
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
			}//end if
		}//end foreach
	}

	private void RemovePlayerMovementFlag()
	{
		GameObject.Destroy (this.m_PlayerMovementFlag_Instance);
		this.m_Movement = Vector3.zero;
	}

	/**KEYBOARD MOVEMENT;
	 * A function to be called from the InputManager to translate the gamer input into player motion.
	 * Note: We only actually decide HOW the player moves from the function we call from this function.*/
	public void SetMovementFromInput(float horizontal_input, float vertical_input)
	{
		if (horizontal_input != 0.0f || vertical_input != 0.0f) {
			Vector3 movement = new Vector3 ();
			movement += new Vector3 (horizontal_input * this.m_Speed, 0.0f, vertical_input * this.m_Speed);
			movement = this.RegulateMagnitude (movement, this.m_Speed);

			//this is where we choose how to apply the input to the player
			this.GlobalMove(movement);
//			this.LocalMove (movement);
		} else {
			this.m_Movement = Vector3.zero;
		}
	}//end f'n void MovePlayer(float, float, float)

	/**A function to move the player container object in world coordinates*/
	private void GlobalMove(Vector3 movement)
	{
		this.m_Movement = movement * Time.fixedDeltaTime;
	}

	/**A function to move the player container object with respect to where the player is facing (forwards is [this.transform.forward])*/
	private void LocalMove(Vector3 movement)
	{
		Vector3 forward = this.transform.forward * movement.z * Time.fixedDeltaTime;
		Vector3 right = this.transform.right * movement.x * Time.fixedDeltaTime;
		Vector3 final_movement = forward + right;
		this.m_Movement = final_movement;
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
