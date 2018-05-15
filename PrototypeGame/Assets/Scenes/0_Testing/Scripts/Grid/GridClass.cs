//#define TESTING_GRID_FUNCTIONALITIES
//#define TESTING_NEIGHBOR_ASSIGNMENT
#define TESTING_FINDING_OBSTRUCTED_GRIDBOXES

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
				gridbox.name = "Gridbox_" + box_index;
				Vector3 adjustment_horizontal = new Vector3 (column * this.m_GridBoxSize.x, 0.0f, 0.0f);
				gridbox.transform.position = adjustment_horizontal + adjustment_vertical;
				gridbox.transform.localScale = this.m_GridBoxSize;

				GridBox box = gridbox.GetComponent<GridBox> ();
				box.InitializeNeighbors (box_index++, this.m_GridBoxesPerFloorX, this.m_GridBoxesPerFloorZ);
				box.VerifyObstructionStatus ();
				this.m_Grid.Add (box);
			}
		}//end for

		#if TESTING_NEIGHBOR_ASSIGNMENT
		for (int index = 0; index < this.m_Grid.Count; index++) {
			this.m_Grid [index].PrintNeighbors (index);
		}
		#endif
	}//end f'n InitializeGrid()

	/**A function intended to be called from the PlayerMovement class - finds and returns the path the player will need to traverse in order to bypass an obstructable, as a List of GridBox objects*/
	public List<GridBox> FindPath(int index_from, int index_to)
	{
		this.CheckGridBoxesAndAssignDistances (index_to);

		List<GridBox> path = new List<GridBox> ();
		return path;

	}

	public bool ObstructableAlongTrajectory(int current_index, int target_index)
	{
		if (current_index == target_index) {
			return false;
		}

		//We want to find the "area" within the indices that we want to examine.
		int row_count = 0, column_count = 0;

		int current_index_column = current_index % this.m_GridBoxesPerFloorX;
		int target_index_column = target_index % this.m_GridBoxesPerFloorX;
		column_count = current_index_column - target_index_column;

		int current_index_row = current_index / this.m_GridBoxesPerFloorX;
		int target_index_row = target_index / this.m_GridBoxesPerFloorX;
		row_count = current_index_row - target_index_row;

		#if TESTING_FINDING_OBSTRUCTED_GRIDBOXES
		string message = "";
		message += "Current index: " + current_index + "; [" + current_index_row + ", " + current_index_column + "]\n";
		message += "Target index: " + target_index + "; [" + target_index_row + ", " + target_index_column + "]\n";
		message += "Indices under investigation:\n";
		#endif
		/*
		 * from the above, we can conclude:
		 * - if row count < 0, then we're moving "downwards" in the grid; target_row > current_row
		 * - if row count == 0, then we're not moving "vertically" in the grid; target_row == current_row
		 * - if row count > 0, then we're moving "upwards" in the grid; target_row < current_row
		 * - if column count < 0, then we're moving "rightwards" in the grid; target_column > current_column
		 * - if column count == 0, then we're not moving "horizontally" in the grid; target_column == current_column
		 * - if column count > 0, then we're moving "leftwards" in the grid; target_column < current_column
		 */

		int row_negative = row_count > 0 ? -1 : 1;
		int column_negative = column_count > 0 ? -1 : 1;

		for (int row = 0; row < Mathf.Abs (row_count); row++) {
			for (int column = 0; column < Mathf.Abs (column_count); column++) {
				int index_under_investigation = current_index + (column * column_negative) + (this.m_GridBoxesPerFloorX * row * row_negative);
				//For first click, we have player gridbox index mapped to -1
				if (index_under_investigation < 0) {
					#if TESTING_FINDING_OBSTRUCTED_GRIDBOXES
					Debug.Log ("Negative index");
					#endif
					return false;
				}
				#if TESTING_FINDING_OBSTRUCTED_GRIDBOXES
				message += " " + index_under_investigation;
				#endif
				bool gridbox_obstructed = this.m_Grid[index_under_investigation].IsGridBoxObstructed();
				if (gridbox_obstructed) {
					#if TESTING_FINDING_OBSTRUCTED_GRIDBOXES
					message += "\nGridbox " + index_under_investigation + " found to be obstructed - returning true";
					Debug.Log (message);
					#endif
					return true;
				}
			}//end for
			#if TESTING_FINDING_OBSTRUCTED_GRIDBOXES
			message += "\n";
			#endif
		}
		#if TESTING_FINDING_OBSTRUCTED_GRIDBOXES
		Debug.Log (message);
		#endif
		//if we make it this far, then none of the grid boxes were obstructed
		return false;
	}

	/**A recursive function to go through all the movement flag's neighboring slots and assign them an integer distance from the flag.
	*The pathfinding algorithm will use these distances to choose the nearest path to the flag.*/
	private void CheckGridBoxesAndAssignDistances(int index)
	{
		int distance = 0;
		this.m_Grid[index].SetGridboxDistanceFromFlag(distance);
		distance++;
		List<int> neighboring_indices = this.m_Grid [index].GetNeighborIndices ();

		foreach (int neighbor in neighboring_indices) {
			if (neighbor > -1) {
				this.m_Grid [neighbor].SetGridboxDistanceFromFlag (distance);
			}
		}
		foreach (int neighbor in neighboring_indices) {
			if (neighbor > -1) {
				this.CheckGridBoxesAndAssignDistances (neighbor);
			}
		}
	}

	/**A function to reset the grid after each pathfinding search*/
	private void ResetGridCheckedStatus()
	{
		foreach (GridBox box in this.m_Grid) {
			box.ResetGridboxDistanceFromFlag();
		}
	}
}
