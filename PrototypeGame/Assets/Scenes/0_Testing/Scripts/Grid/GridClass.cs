//#define TESTING_GRID_FUNCTIONALITIES
//#define TESTING_NEIGHBOR_ASSIGNMENT
//#define TESTING_FINDING_OBSTRUCTED_GRIDBOXES
//#define TESTING_NATURALIZED_PATH_ACQUISITION
//#define TESTING_MANHATTAN_PATHFINDING

// #define EXPERIMENTATION_MANHATTAN
#define EXPERIMENTATION_DJIKSTRA

#if EXPERIMENTATION_MANHATTAN
#undef EXPERIMENTATION_DJIKSTRA
#elif EXPERIMENTATION_DJIKSTRA
#undef EXPERIMENTATION_MANHATTAN
#endif

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


	/**An int value to represent the current distance from the flag for the neighbors whose indices are in the neighbor list.*/
	private int m_CurrentDistanceFromFlag = 0;
	/**A list containing all neighbors up for pathfinding inspection.
	*Note: this property should be private in the final iteration.*/
	private List<int> m_NeighborList = new List<int> ();

	/**The list of gridboxes currently up for contention*/
	public List<int> m_OpenList = new List<int>();
	/**The list of gridboxes we know are part of our path*/
	public List<int> m_ClosedList = new List<int>();

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

	#if EXPERIMENTATION_DJIKSTRA
	/**A function intended to be called from the PlayerMovement class - finds and returns the naturalized path the player will need to traverse in order to bypass an obstructable, as a List of GridBox objects*/
	public List<GridBox> FindPath_Naturalized(int index_from, int index_to)
	{
		this.CheckGridBoxesAndAssignDistances (index_to);

		List<GridBox> path = new List<GridBox> ();
		foreach (int list_item in this.FindPathWithSmallestNeighbors(index_from)) {
			path.Add (this.m_Grid[list_item]);
		}

		//we could further "naturalize" this movement by returning a path filled with only the nodes right before an obstructable
		List<GridBox> naturalized_path = new List<GridBox>();
		int start = index_from;
		#if TESTING_NATURALIZED_PATH_ACQUISITION
		string message = "";
		#endif
		for (int index = 0; index < path.Count; index++) {
			if (this.ObstructableAlongTrajectory (start, path[index].GetBoxIndex ())) {
				naturalized_path.Add (path [index - 1]);
				#if TESTING_NATURALIZED_PATH_ACQUISITION
				message += "Obstruction detected between gridbox " + start + " and " + path[index].GetBoxIndex() + ". Added gridbox " + path [index - 1].GetBoxIndex () + " to naturalized path\n";
				#endif
				start = path[index - 1].GetBoxIndex();
			}
		}
		naturalized_path.Add (path [path.Count - 1]);
		#if TESTING_NATURALIZED_PATH_ACQUISITION
		Debug.Log (message);
		#endif
		return naturalized_path;
	}
	#endif

	/**Assigns G value to immediate neighbors so long as those neighbors:
		- Aren't obstructed
		- Are within the grid (don't have index -1)
		- Aren't on the open or the closed list*/
	private void AssignGToImmediateNeighbors(int grid_box, int G)
	{
		#if TESTING_MANHATTAN_PATHFINDING
		string message = "";
		message += "Assigning G value " + G + " for immediate neighbors to slot " + grid_box + ":\t";
		#endif
		foreach (int neighbor in this.m_Grid[grid_box].GetNeighborIndices()) {
			bool neighbor_is_invalid = (neighbor == -1) || this.m_Grid [neighbor].IsGridBoxObstructed ();
			if (!neighbor_is_invalid && !this.GridBoxIsInClosedList(neighbor) && !this.GridBoxIsInOpenList(neighbor)) {
				#if TESTING_MANHATTAN_PATHFINDING
				message += neighbor + " ";
				#endif
				this.m_Grid [neighbor].SetG (G);
			}
		}
		#if TESTING_MANHATTAN_PATHFINDING
		Debug.Log(message);
		#endif
	}

	/**A function to find and assign the H-value, the distance to the [destination_index] gridbox, for the given [grid_box] index's immediate neighbors*/
	private void ComputeHForImmediateNeighbors(int grid_box, int destination_index)
	{
		int destination_row = (destination_index / this.m_GridBoxesPerFloorX) + 1;
		int destination_column = (destination_index % this.m_GridBoxesPerFloorX) + 1;

		//The sum of the difference in rows and columns between a given grid box and the destination is equal to the distance covered
		//between the two.
		foreach (int neighbor_index in this.m_Grid[grid_box].GetNeighborIndices()) {
			bool neighbor_is_invalid = (neighbor_index == -1) || this.m_Grid [neighbor_index].IsGridBoxObstructed ();
			if (!neighbor_is_invalid && !this.GridBoxIsInClosedList(neighbor_index) && !this.GridBoxIsInOpenList(neighbor_index)) {
				int neighbor_gridbox_row = (neighbor_index / this.m_GridBoxesPerFloorX) + 1;
				int neighbor_gridbox_column = (neighbor_index % this.m_GridBoxesPerFloorX) + 1;

				int total_distance = Mathf.Abs (neighbor_gridbox_row - destination_row) + Mathf.Abs (neighbor_gridbox_column - destination_column);
				this.m_Grid [neighbor_index].SetH (total_distance);
			}
		}
	}

	/**A function to let us know whether the given grid box index is contained in the open list*/
	private bool GridBoxIsInOpenList(int grid_box)
	{
		foreach (int index in this.m_OpenList) {
			if (grid_box == index) {
				return true;
			}
		}
		return false;
	}

	/**A function to let us know whether the given grid box index is contained in the closed list*/
	private bool GridBoxIsInClosedList(int grid_box)
	{
		foreach (int index in this.m_ClosedList) {
			if (grid_box == index) {
				return true;
			}
		}
		return false;
	}

	private void AddImmediateNeighborsToOpenList(int index_from)
	{
		#if TESTING_MANHATTAN_PATHFINDING
		string message = "Added to open list immediate neighbors to slot " + index_from + " :\t";
		#endif
		foreach (int neighboring_index in this.m_Grid[index_from].GetNeighborIndices()) {
			bool neighbor_is_invalid = (neighboring_index == -1) || this.m_Grid [neighboring_index].IsGridBoxObstructed ();
			if (!neighbor_is_invalid && !this.GridBoxIsInClosedList(neighboring_index) && !this.GridBoxIsInOpenList(neighboring_index)) {
				this.m_OpenList.Add (neighboring_index);
				#if TESTING_MANHATTAN_PATHFINDING
				message += neighboring_index + " ";
				#endif
			}
		}
		#if TESTING_MANHATTAN_PATHFINDING
		Debug.Log(message);
		#endif
	}

	/**A function to compute the F values for every element in the open list whose F value is still set to default value 0*/
	private void ComputeFValuesForOpenList()
	{
		foreach (int grid_box_index in this.m_OpenList) {
			GridBox grid_box = this.m_Grid [grid_box_index];
			if (grid_box.GetF () == 0) {
				grid_box.ComputeF ();
			}
		}
	}

	/**A function to find the immediate valid neighbor slot with the lowest and most recently added F value, add it to the closed list, and remove it from the open list*/
	private void FindLowestFAndAddToClosedList()
	{
		#if TESTING_MANHATTAN_PATHFINDING
		string message = "";
		#endif

		//Default initial values
		int index_of_lowest_F = -1;

		GridBox closedlist_last_gridbox = this.m_Grid [this.m_ClosedList [this.m_ClosedList.Count - 1]];
//		int value_of_lowest_F = closedlist_last_gridbox.GetF();
		int value_of_lowest_F = this.m_GridBoxesPerFloorX * this.m_GridBoxesPerFloorZ;
//		if (this.m_ClosedList.Count == 1) {
//			value_of_lowest_F = this.m_GridBoxesPerFloorX * this.m_GridBoxesPerFloorZ;
//		}
		int value_of_greatest_G = closedlist_last_gridbox.GetG ();
		//Recall that the most recently added entries will be at the end of the list
		for (int index = 0; index < this.m_OpenList.Count; index++) {
//		for (int index = this.m_OpenList.Count - 1; index >= 0; index--) {
			GridBox openlist_gridbox = this.m_Grid [this.m_OpenList [index]];
			if (openlist_gridbox.GetF () <= value_of_lowest_F && openlist_gridbox.GetG() > closedlist_last_gridbox.GetG()) {
				index_of_lowest_F = index;
				value_of_lowest_F = openlist_gridbox.GetF ();
			}
		}

		if (index_of_lowest_F == -1) {
			index_of_lowest_F = this.m_OpenList.Count - 1;
		}

		//now we add the lowest-valued F gridbox index of the open list to the closed list
		this.m_ClosedList.Add (this.m_OpenList [index_of_lowest_F]);

		#if TESTING_MANHATTAN_PATHFINDING
		message += "Slot with lowest F found to be: " + this.m_Grid[this.m_OpenList [index_of_lowest_F]] + " - adding to closed list";
		Debug.Log(message);
		#endif

		//and then we remove it from the open list
		this.m_OpenList.RemoveAt (index_of_lowest_F);
	}

	private void FindClosedList(int index_from, int index_to, int G)
	{
		//Then assign G and H values to immediate neighbors
		this.AssignGToImmediateNeighbors(index_from, G);
		this.ComputeHForImmediateNeighbors (index_from, index_to);
		//And add them to the open list
		this.AddImmediateNeighborsToOpenList(index_from);
		//Then compute F values
		this.ComputeFValuesForOpenList();
		//Add most recently added and lowest F value to the closed list
		this.FindLowestFAndAddToClosedList();


//		#if TESTING_MANHATTAN_PATHFINDING
//		string message = "";
//		foreach(int neighboring_index in this.m_Grid[index_from].GetNeighborIndices())
//		{
//			bool neighbor_is_invalid = (neighboring_index == -1) || this.m_Grid [neighboring_index].IsGridBoxObstructed ();
//			if (!neighbor_is_invalid) {
//				message += "Neighbor " + neighboring_index + " assigned: " + 
//			}
//		}
//		#endif

		int last_closed_list_entry = this.m_ClosedList [this.m_ClosedList.Count - 1];
		if (last_closed_list_entry != index_to) {
			G++;
			this.FindClosedList (last_closed_list_entry, index_to, G);
		}
	}

	/**A function to clear the information needed to restart the A* Manhattan pathfinding algorithm*/
	public void ResetPathfindingInformation()
	{
		Debug.Log ("Reset pathfinding info");
		foreach (int index in this.m_ClosedList) {
			this.m_Grid [index].SetG (0);
			this.m_Grid [index].SetH (0);
			this.m_Grid [index].ComputeF ();
		}
		foreach (int index in this.m_OpenList) {
			this.m_Grid [index].SetG (0);
			this.m_Grid [index].SetH (0);
			this.m_Grid [index].ComputeF ();
		}

		this.m_ClosedList.Clear ();
		this.m_OpenList.Clear ();
	}

	#if EXPERIMENTATION_MANHATTAN
	/**A function intended to be called from the PlayerMovement class - finds and returns the naturalized path the player will need to traverse in order to bypass an obstructable, as a List of GridBox objects*/
	public List<GridBox> FindPath_Naturalized(int index_from, int index_to)
	{
		//First add starting position to closed list
		this.m_ClosedList.Add(index_from);
		int G = 1;

		this.FindClosedList (index_from, index_to, G);

		//we could further "naturalize" this movement by returning a path filled with only the nodes right before an obstructable
		List<GridBox> naturalized_path = new List<GridBox>();
		int start = index_from;
		#if TESTING_NATURALIZED_PATH_ACQUISITION
		string message = "";
		#endif
		for (int index = 0; index < this.m_ClosedList.Count; index++) {
			if (this.ObstructableAlongTrajectory (start, this.m_ClosedList[index])) {
				naturalized_path.Add (this.m_Grid[this.m_ClosedList[index - 1]]);
//				naturalized_path.Add (path [index - 1]);
				#if TESTING_NATURALIZED_PATH_ACQUISITION
//				message += "Obstruction detected between gridbox " + start + " and " + path[index].GetBoxIndex() + ". Added gridbox " + path [index - 1].GetBoxIndex () + " to naturalized path\n";
				message += "Obstruction detected between gridbox " + start + " and " + this.m_ClosedList[index] + ". Added gridbox " + this.m_ClosedList[index - 1] + " to naturalized path\n";
				#endif
//				start = path[index - 1].GetBoxIndex();
				start = this.m_ClosedList[index - 1];
			}
		}
		naturalized_path.Add (this.m_Grid [this.m_ClosedList [this.m_ClosedList.Count - 1]]);
		#if TESTING_NATURALIZED_PATH_ACQUISITION
		Debug.Log (message);
		#endif
		return naturalized_path;
	}
	#endif

	/**A function intended to be called from the PlayerMovement class - finds and returns the path (without naturalization) the player will need to traverse in order to bypass an obstructable, as a List of GridBox objects*/
	public List<GridBox> FindPath(int index_from, int index_to)
	{
		//First add starting position to closed list
		this.m_ClosedList.Add(index_from);
		int G = 1;

		this.FindClosedList (index_from, index_to, G);
		List<GridBox> templist = new List<GridBox> ();
		foreach (int index in this.m_ClosedList) {
			templist.Add (this.m_Grid[index]);
		}
		return templist;
//		this.CheckGridBoxesAndAssignDistances (index_to);
//
//		List<GridBox> path = new List<GridBox> ();
//		foreach (int list_item in this.FindPathWithSmallestNeighbors(index_from)) {
//			path.Add (this.m_Grid[list_item]);
//		}
//
//		return path;
	}

	private List<int> FindPathWithSmallestNeighbors(int start_index)
	{
		List<int> list_to_return = new List<int> ();
		list_to_return.Add(this.FindIndexOfSmallestNeighbor(start_index));
		for (int length = 1; length < this.m_Grid [start_index].GetGridboxDistanceFromFlag(); length++) {
			list_to_return.Add (this.FindIndexOfSmallestNeighbor (list_to_return [list_to_return.Count - 1]));
		}
		return list_to_return;
	}

	private int FindIndexOfSmallestNeighbor(int start_index)
	{
		//placeholder, until we actually find the smallest value
		int index_of_smallest = start_index;
		foreach (int neighbor in this.m_Grid[start_index].GetNeighborIndices()) {
			if (neighbor > -1 && this.m_Grid [neighbor].GetGridboxDistanceFromFlag () > -1) {
				//if there exists a neighbor with distance smaller than the smallest, it becomes the new smallest
				int neighbor_distance = this.m_Grid [neighbor].GetGridboxDistanceFromFlag ();
				if (neighbor_distance < this.m_Grid [index_of_smallest].GetGridboxDistanceFromFlag ()) {
					index_of_smallest = neighbor;
				}
			}
		}
		return index_of_smallest;
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

//		for (int row = 0; row < Mathf.Abs (row_count); row++) {
//			for (int column = 0; column < Mathf.Abs (column_count); column++) {
		for (int row = 0; row <= Mathf.Abs (row_count); row++) {
			for (int column = 0; column <= Mathf.Abs (column_count); column++) {
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
		if (index > -1){
			//First check the current index
			GridBox grid_box = this.m_Grid [index];
			//if this is the first grid box we check...
			if (this.m_CurrentDistanceFromFlag == 0) {
				//...then set the distance for the flag box to 0 and increment the distance value
				grid_box.SetGridboxDistanceFromFlag (this.m_CurrentDistanceFromFlag++);

				foreach (int neighboring_index in grid_box.GetNeighborIndices()) {
					if (this.GridBoxIsValidForPathfinding (neighboring_index)) {
						this.m_NeighborList.Add (neighboring_index);
					}
				}
			}

			this.ExhaustNeighbors ();
		}

	}

	/**A function to recursively exhaust all neighbors found during the pathfinding setup*/
	private void ExhaustNeighbors()
	{
		if (this.m_NeighborList.Count > 0) {

			List<int> temp_neighbor_list = new List<int> ();
			foreach (int neighbor in this.m_NeighborList) {
				if (neighbor > -1) {
					foreach (int neighbor_neighbor in this.m_Grid[neighbor].GetNeighborIndices()) {
						if (this.GridBoxIsValidForPathfinding(neighbor_neighbor))
						{
							temp_neighbor_list.Add (neighbor_neighbor);
						}
					}
					this.m_Grid [neighbor].SetGridboxDistanceFromFlag (this.m_CurrentDistanceFromFlag);
				}
			}

			this.m_CurrentDistanceFromFlag++;

			this.m_NeighborList.Clear ();
			foreach (int neighbor in temp_neighbor_list) {
				this.m_NeighborList.Add (neighbor);
			}

			this.ExhaustNeighbors ();
		}

	}

	/**A function to ensure a gridbox can be used for pathfinding.
	*Specifically, ensures that the gridbox has a proper positive index value, that the gridbox isn't obstructed, and that the gridbox hasn't yet been checked for pathfinding.
	*Note: If a gridbox has been assigned a distance from the flag gridbox, we say of it that it has been checked for pathfinding.*/
	private bool GridBoxIsValidForPathfinding(int gridbox_index)
	{
		bool grid_box_exists = (gridbox_index != -1);
		if (!grid_box_exists) {
//			Debug.Log ("grid box doesn't exist");
			return false;
		} else {
			GridBox grid_box = this.m_Grid [gridbox_index];
//			Debug.Log ("grid box is not obstructed: " + !grid_box.IsGridBoxObstructed () + " grid box has not been checked for pathfinding: " + !grid_box.GridboxHasBeenCheckedForPathfinding ());
			return !grid_box.IsGridBoxObstructed () && !grid_box.GridboxHasBeenCheckedForPathfinding ();
		}
	}


	/**A function to reset the grid after each pathfinding search
	*/
	public void ResetGridCheckedStatus()
	{
		foreach (GridBox box in this.m_Grid) {
			box.ResetGridboxDistanceFromFlag();
		}
		this.m_CurrentDistanceFromFlag = 0;
		this.m_NeighborList.Clear ();
	}


}
