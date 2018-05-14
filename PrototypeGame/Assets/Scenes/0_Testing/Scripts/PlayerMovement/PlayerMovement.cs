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
//	private Vector3 m_Movement = Vector3.zero;
	/**The direction in which the player is moving.*/
	private Vector3 m_DirectionOfMotion = Vector3.zero;

	/**The maximal speed at which the player moves (once they've accelerated as much as they can)*/
	public float m_TerminalVelocity;
	/**The rate at which the current velocity grows when the player is moving*/
	public float m_Acceleration;
	/**The time it takes, once deceleration is triggered, for the player to stop*/
	public float m_TimeTillRest;
	/**The rate at which the current velocity slows when the player is arriving somewhere.*/
	private float m_Deceleration;
	/**The current velocity at which the player is moving.*/
	public float m_CurrentVelocity = 0.0f;


	/**The radius of the movement flag planted where the player needs to go; the smaller this is, the closer to the center of a gridbox the player will move*/
	private float m_MovementFlagInstanceRadius = 1.0f;

	private Animator m_PlayerMovementAnimator;

	private const string ANIMATOR_PARAM_ISMOVING_LEFT = "isMovingLeft";
	private const string ANIMATOR_PARAM_ISMOVING_RIGHT = "isMovingRight";
	private const string ANIMATOR_PARAM_ISMOVING_UP = "isMovingUp";
	private const string ANIMATOR_PARAM_ISMOVING_DOWN = "isMovingDown";

	/**The index of the Gridbox currently occupied by the player
	 * Note that while the player is moving, this quantity will reflect the grid box they are moving to. 
	 * This quantity is public mostly for testing.
	*/
	public int m_GridBoxCurrentIndex = -1;

	void Awake()
	{
		this.m_PlayerMovementAnimator = this.GetComponentInChildren<Animator> ();
//		this.m_MovementFlagInstanceRadius = this.m_PlayerMovementFlag_Prefab.GetComponent<SphereCollider> ().radius;
	}

	void Update()
	{
		this.ApplyMovement();
		this.UpdateAnimator ();
	}

	/**A function to manage the effect of acceleration on the player's velocity (when they start moving, specifically).
	*Should be called somewhere the player's movement is triggered.*/
	private void ApplyAccelerationToCurrentVelocity()
	{
		if (this.m_CurrentVelocity < this.m_TerminalVelocity) {
			this.m_CurrentVelocity += this.m_Acceleration * Time.fixedDeltaTime;
		} else if (this.m_CurrentVelocity > this.m_TerminalVelocity) {
			this.m_CurrentVelocity = this.m_TerminalVelocity;
		}
	}

	/**A function to manage the effect of deceleration on the player's velocity (when they stop moving, specifically).
	*Should probably be called somewhere based on the current velocity such that the player stops at the right place.*/
	private void ApplyDecelerationToCurrentVelocity()
	{
		if (this.m_CurrentVelocity > 0.0f) {
			this.m_CurrentVelocity -= this.m_Deceleration * Time.fixedDeltaTime;
		} 
		else if (this.m_CurrentVelocity < 0.0f) {
			this.m_CurrentVelocity = 0.0f;
			RemovePlayerMovementFlag ();
			this.m_Deceleration = 0.0f;
		}
	}

	/**A function to manage the acceleration, deceleration, and motion we apply to the player*/
	private void ApplyMovement()
	{
		// If motion has been initiated
		if (this.m_PlayerMovementFlag_Instance != null) {

			//In the more primitive version of motion, we set the direction right from the get-go, from the flag position.

			//If the player should be accelerating...
			if (this.PlayerShouldBeAccelerating ()) {
//				Debug.Log ("Player should be accelerating");
				//...begin by finding the direction of the motion, if we don't know it yet
				if (this.WeNeedANewDirectionOfMotion ()) {
					this.FindAndNormalizeDirectionOfMotion ();
				}
				//...then apply acceleration to the current velocity vector
				this.ApplyAccelerationToCurrentVelocity();
			} 
			//else if the player should be decelerating...
			else {
//				Debug.Log ("Player should be decelerating");
				//...first find the value the deceleration must have, if we don't know it yet
				if (this.DecelerationMustBeFound ()) {
					this.FindDeceleration ();
				}
				//...and then apply it
				this.ApplyDecelerationToCurrentVelocity();
				//...and if the player is where they need to be, reset the deceleration and remove the movement flag
				if (this.PlayerHasArrivedToDestination ()) {
					//Reset deceleration
					this.m_Deceleration = 0.0f;
					//Destroy movement flag
					this.RemovePlayerMovementFlag ();
					return;
				}
			}//end else

			this.ApplyVelocityAndDirectionOfMotionToPlayer ();
		}//end if flag exists
	}

	/**A function to properly apply the motion resulting from all the wizardry we worked on the current velocity onto the player gameobject*/
	private void ApplyVelocityAndDirectionOfMotionToPlayer()
	{
		//We already apply our Time.fixedDeltatime in our acceleration functions, so no need to do it here.
		Vector3 motion = this.m_DirectionOfMotion * this.m_CurrentVelocity;
		this.transform.position += motion;
	}

	/**A function to tell us whether or no we need to update our direction of motion.
	*This function will grow more complex with the addition of the pathfinding, but for now, the only information we need is whether or not we're at rest.*/
	private bool WeNeedANewDirectionOfMotion()
	{
		Vector3 player_to_flag = this.m_PlayerMovementFlag_Instance.transform.position - this.transform.position;
		Vector2 twoD_player_to_flag = new Vector2 (player_to_flag.x, player_to_flag.z);
		Vector2 current_orientation_of_motion = new Vector2 (this.m_DirectionOfMotion.x, this.m_DirectionOfMotion.z);
		bool we_are_moving_in_the_wrong_direction = Vector2.Dot (twoD_player_to_flag, current_orientation_of_motion) != 1.0f;

		//For instance, we could say we update this for every gridbox we pass through, in the later stages

		return this.m_CurrentVelocity == 0.0f && we_are_moving_in_the_wrong_direction;
	}

	/**Update and normalize the direction in which we're moving.
	*Note that this is going to be working along the XZ plane, specifically.*/
	private void FindAndNormalizeDirectionOfMotion()
	{
		Vector2 current_position = new Vector2 (this.transform.position.x, this.transform.position.z);
		Vector2 flag_position = new Vector2 (this.m_PlayerMovementFlag_Instance.transform.position.x, this.m_PlayerMovementFlag_Instance.transform.position.z);
		Vector2 twoD_directionOfMotion = flag_position - current_position;
		this.m_DirectionOfMotion = new Vector3 (twoD_directionOfMotion.x, 0.0f, twoD_directionOfMotion.y);
		this.m_DirectionOfMotion.Normalize ();
	}

	/**A function to tell us whether or not the player has arrived to their destination. 
	 * Keeping in mind that we calculate our deceleration such that the player comes to rest at the right place, then in applying our deceleration we should be at the right place when the player's velocity is null.
	*/
	private bool PlayerHasArrivedToDestination()
	{
		return this.m_CurrentVelocity == 0.0f;
	}

	/**A function to tell us whether or not the player should be in the process of accelerating.
	*We're accelerating where a movement flag instance has been triggered and we're not within the radius of the prefab*/
	private bool PlayerShouldBeAccelerating()
	{
		Vector2 current_position = new Vector2 (this.transform.position.x, this.transform.position.z);
		Vector2 flag_position = new Vector2 (this.m_PlayerMovementFlag_Instance.transform.position.x, this.m_PlayerMovementFlag_Instance.transform.position.z); 
		Vector2 flag_to_current_position = flag_position - current_position;
		float distance_to_flag = flag_to_current_position.magnitude;
		return (distance_to_flag > this.m_MovementFlagInstanceRadius);
	}

	/**A function to tell us whether or not the deceleration should be found at this point.
	*If we're at this point in the program and the deceleration has value 0, it must be found.*/
	private bool DecelerationMustBeFound()
	{
		return (this.m_Deceleration == 0.0f);
	}
		
	/**A function called to find the deceleration quantity; this is what we will be subtracting from the current velocity such that the player comes to rest at the center of the gridbox*/
	private void FindDeceleration()
	{
		float distance_to_cover = this.m_PlayerMovementFlag_Instance.GetComponent<SphereCollider>().radius;

		this.m_CurrentVelocity = (2.0f * distance_to_cover) / this.m_TimeTillRest * Time.fixedDeltaTime;
		this.m_Deceleration = this.m_CurrentVelocity / this.m_TimeTillRest;
	}

	/**A function to update the player animator, with respect to the movement input (specifically, with respect to [this.m_Movement]*/
	private void UpdateAnimator()
	{
		this.m_PlayerMovementAnimator.SetBool (ANIMATOR_PARAM_ISMOVING_LEFT, this.m_DirectionOfMotion.x < 0.0f);
		this.m_PlayerMovementAnimator.SetBool (ANIMATOR_PARAM_ISMOVING_RIGHT, this.m_DirectionOfMotion.x > 0.0f);
		this.m_PlayerMovementAnimator.SetBool (ANIMATOR_PARAM_ISMOVING_UP, this.m_DirectionOfMotion.z > 0.0f);
		this.m_PlayerMovementAnimator.SetBool (ANIMATOR_PARAM_ISMOVING_DOWN, this.m_DirectionOfMotion.z < 0.0f);
		//Note: Idle behavior is defined as an absence of any motion
	}

	/**
	 * A function to create the flag designating where the player gameobject is moving towards*/
	public void CreatePlayerMovementFlag()
	{
		//If there's already a flag when we trigger the flag creation, destroy the currently existing flag to make room for the new one
		if (this.m_PlayerMovementFlag_Instance != null) {
			RemovePlayerMovementFlag ();
			this.m_CurrentVelocity /= 10.0f;
		}
		//Create a flag where we clicked
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		foreach (RaycastHit hit in Physics.RaycastAll(ray)) {
			GridBox grid_box = hit.collider.gameObject.GetComponent<GridBox> ();
			//If what was clicked was an unobstructed gridbox...
			if (grid_box != null && !grid_box.IsGridBoxObstructed()) {
				this.m_PlayerMovementFlag_Instance = GameObject.Instantiate (this.m_PlayerMovementFlag_Prefab);
				this.m_PlayerMovementFlag_Instance.transform.GetComponent<SphereCollider> ().radius = this.m_MovementFlagInstanceRadius;
				//Move movement flag to gridbox position
				this.m_PlayerMovementFlag_Instance.transform.position = grid_box.transform.position;
				//Update direction of motion
				this.FindAndNormalizeDirectionOfMotion();
				//Update the index of the current grid box we're occupying
				this.m_GridBoxCurrentIndex = grid_box.GetBoxIndex ();
			}
		}//end foreach
	}

	private void RemovePlayerMovementFlag()
	{
		GameObject.Destroy (this.m_PlayerMovementFlag_Instance);
		this.m_CurrentVelocity = 0.0f;
	}

//	/**KEYBOARD MOVEMENT;
//	 * A function to be called from the InputManager to translate the gamer input into player motion.
//	 * Note: We only actually decide HOW the player moves from the function we call from this function.*/
//	public void SetMovementFromInput(float horizontal_input, float vertical_input)
//	{
//		if (horizontal_input != 0.0f || vertical_input != 0.0f) {
//			this.ApplyAcceleration ();
//			Vector2 movement = new Vector2(horizontal_input, vertical_input);
//			movement.Normalize ();
//			movement *= this.m_CurrentVelocity;
//
//			//this is where we choose how to apply the input to the player
//			this.GlobalMove(new Vector3(movement.x, 0.0f, movement.y));
////			this.LocalMove (new Vector3(movement.x, 0.0f, movement.z));
//		} else {
//			this.m_Movement = Vector3.zero;
//		}
//	}//end f'n void MovePlayer(float, float, float)
//
//	/**A function to move the player container object in world coordinates*/
//	private void GlobalMove(Vector3 movement)
//	{
//		this.m_Movement = movement * Time.fixedDeltaTime;
//	}
//
//	/**A function to move the player container object with respect to where the player is facing (forwards is [this.transform.forward])*/
//	private void LocalMove(Vector3 movement)
//	{
//		Vector3 forward = this.transform.forward * movement.z * Time.fixedDeltaTime;
//		Vector3 right = this.transform.right * movement.x * Time.fixedDeltaTime;
//		Vector3 final_movement = forward + right;
//		this.m_Movement = final_movement;
//	}
		

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
