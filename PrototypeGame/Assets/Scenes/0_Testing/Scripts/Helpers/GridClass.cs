using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridClass : MonoBehaviour {
	/**The floor the grid is meant to be mapping to.*/
	[SerializeField] GameObject m_Floor;
	/**The position at which the grid will begin initializing; going from top-left to bottom-right.*/
	private Vector3 m_StartingPosition = Vector3.zero;

	/**The size of each individual grid box; to be defined from the Inspector, with respect to the desired quantity of grid boxes*/
	private Vector3 m_GridBoxSize = new Vector3 ();
	/**Grid boxes desired along floor width; to be set from the Inspector*/
	public int m_GridBoxesPerFloorX;
	/**Grid boxes desired along floor "height"; to be set from the Inspector*/
	public int m_GridBoxesPerFloorZ;

	void Awake()
	{
//		float start_x = this.m_Floor.transform.position.x + 2.0f;
//		float start_y = this.m_Floor.transform.position.y + 0.5f;
//		float start_z = this.m_Floor.transform.position.z + 0.5f;
//		this.m_StartingPosition = new Vector3 (start_x, start_y, start_z);
	}

	/**A tester function; will draw what the grid should resemble, in Editor mode only*/
	void OnDrawGizmos()
	{
		//Define grid box size with respect to floor size. Assume we're working with a rectangle
		//How many cubes do we want along the length of the floor? And along the width?
		this.m_GridBoxSize.x = this.m_Floor.transform.lossyScale.x / this.m_GridBoxesPerFloorX;
		this.m_GridBoxSize.z = this.m_Floor.transform.lossyScale.z / this.m_GridBoxesPerFloorZ;

		//Start at top left, looking on XZ plane, where "top-left" is X<0, Z>0
		float start_x = this.m_Floor.transform.position.x - ((this.m_Floor.transform.lossyScale.x - this.m_GridBoxSize.x) / 2.0f);
		float start_y = this.m_Floor.transform.position.y + ((this.m_Floor.transform.lossyScale.y + this.m_GridBoxSize.y) / 2.0f);
		float start_z = this.m_Floor.transform.position.z + ((this.m_Floor.transform.lossyScale.z - this.m_GridBoxSize.z) / 2.0f);
		this.m_StartingPosition = new Vector3 (start_x, start_y, start_z);

		//Draw the whole grid
		for (int row = 0; row < this.m_GridBoxesPerFloorZ; row++) {
			Vector3 adjustment_vertical = this.m_StartingPosition - new Vector3 (0.0f, 0.0f, this.m_GridBoxSize.z * row);
			for (int column = 0; column < this.m_GridBoxesPerFloorX; column++) {
				Gizmos.color = (column % 2 == 0) ? new Color (1.0f, 0.0f, 0.0f) : new Color (0.0f, 0.0f, 1.0f);
				Vector3 adjustment_horizontal = new Vector3 (column * this.m_GridBoxSize.x, 0.0f, 0.0f);
				Gizmos.DrawCube (adjustment_horizontal + adjustment_vertical, this.m_GridBoxSize);
			}
		}

	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
