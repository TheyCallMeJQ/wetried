//#define TESTING_GRID_FUNCTIONALITIES
#define TESTING_NEIGHBOR_ASSIGNMENT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridClass : MonoBehaviour {
	/**The floor the grid is meant to be mapping to.*/
	[SerializeField] GameObject m_Floor;
	/**A reference to the actual gridbox prefab*/
	[SerializeField] GameObject m_GridBox_Prefab;
	/**The list of all grid boxes; the grid.*/
	private List<GridBox> m_Grid = new List<GridBox>();

	/**The position at which the grid will begin initializing; going from top-left to bottom-right.*/
	private Vector3 m_StartingPosition = Vector3.zero;

	/**The size of each individual grid box; to be defined from the Inspector, with respect to the desired quantity of grid boxes*/
	private Vector3 m_GridBoxSize = new Vector3 ();
	/**Grid boxes desired along floor width; to be set from the Inspector.
	*This quantity is effectively the number of grid boxes per row.*/
	public int m_GridBoxesPerFloorX;
	/**Grid boxes desired along floor "height"; to be set from the Inspector
	*This quantity is effectively the number of grid boxes per column.*/
	public int m_GridBoxesPerFloorZ;

	void Awake()
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
	}

	#if TESTING_GRID_FUNCTIONALITIES
	/**A tester function; will draw what the grid should resemble, in Editor mode only.
	*Should only be uncommented for testing.*/
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
	#else
	void Start()
	{
		this.InitializeGrid ();
	}
	#endif

	/**A function to create the grid illustrated by the gizmo function OnDrawGizmos().
	*Creates a grid of Gridbox objects.*/
	public void InitializeGrid()
	{
		GameObject grid_hierarchy_container = new GameObject ();
		grid_hierarchy_container.gameObject.name = "Grid Container";

		int box_index = 0;
		//Draw the whole grid
		for (int row = 0; row < this.m_GridBoxesPerFloorZ; row++) {
			Vector3 adjustment_vertical = this.m_StartingPosition - new Vector3 (0.0f, 0.0f, this.m_GridBoxSize.z * row);
			for (int column = 0; column < this.m_GridBoxesPerFloorX; column++) {
				GameObject gridbox = GameObject.Instantiate (this.m_GridBox_Prefab, grid_hierarchy_container.transform);
				Vector3 adjustment_horizontal = new Vector3 (column * this.m_GridBoxSize.x, 0.0f, 0.0f);
				gridbox.transform.position = adjustment_horizontal + adjustment_vertical;
				gridbox.transform.localScale = this.m_GridBoxSize;

				GridBox box = gridbox.GetComponent<GridBox> ();
				box.InitializeNeighbors (box_index++, this.m_GridBoxesPerFloorX, this.m_GridBoxesPerFloorZ);
				this.m_Grid.Add (box);
			}
		}//end for

		#if TESTING_NEIGHBOR_ASSIGNMENT
		for (int index = 0; index < this.m_Grid.Count; index++) {
			this.m_Grid [index].PrintNeighbors (index);
		}
		#endif
	}
}
