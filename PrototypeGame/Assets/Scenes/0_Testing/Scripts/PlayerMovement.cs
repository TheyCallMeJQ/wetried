using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

	/**A function to move the player gameobject by the specified amount*/
	public void Move(Vector3 move_by_what)
	{
//		this.transform.position = this.transform.position + move_by_what;
		this.transform.position += move_by_what;
	}


}
