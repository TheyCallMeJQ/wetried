/*
* Honestly, the entirety of this class belongs within the GridClass class. 
* But if we do that, then we can't attach this script to the prefab instance, so ignore me.
*/

//#define TESTING_NEIGHBOR_INDICES

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBox : MonoBehaviour {
	private int m_Index = -1;
	private enum NEIGHBOR_POSITIONS {TOP = 0, RIGHT = 1, BOTTOM = 2, LEFT = 3};
	private List<int> m_NeighborIndices = new List<int>();
	/**A value to tell us whether or not there's something that would potentially inhibit movement at this gridbox.*/
	public bool m_IsObstructed = false;

	public float m_HeightOfObstructionCheck = 30.0f;

	/**An int value representing the distance from the target the player is moving to, along the defined neighbors, and excluding passage through obstructed gridboxes.
	*NOTE: Should be private in final rendition.*/
	public int m_DistanceFromDestination = -1;

	/**A function to have the gridbox check whether or not it is obstructed by anything.
	* Note: we count anything under the "Obstructable" layer as being something that can obstruct the gridbox.
	* If a gridbox is obstructed, the player can't walk there.
	*/
	public void VerifyObstructionStatus()
	{
		float boxgrid_width_half = this.transform.lossyScale.x / 2.0f;
		float boxgrid_depth_half = this.transform.lossyScale.z / 2.0f;

		for (int corner = 0; corner < 4; corner++) {
			Vector3 origin = this.transform.position - new Vector3(0.0f, 0.1f, 0.0f);
			switch (corner) {
			//Top right corner
			case 0:
				{
					origin += new Vector3 (boxgrid_width_half, 0.0f, boxgrid_depth_half);
					break;
				}
			//Top left corner
			case 1:
				{
					origin += new Vector3 (-boxgrid_width_half, 0.0f, boxgrid_depth_half);
					break;
				}
			//Bottom left corner
			case 2:
				{
					origin += new Vector3 (boxgrid_width_half, 0.0f, -boxgrid_depth_half);
					break;
				}
			//Bottom right corner
			case 3:
				{
					origin += new Vector3 (-boxgrid_width_half, 0.0f, -boxgrid_depth_half);
					break;
				}
			default:
				{
					Debug.Log ("Error in GridBox::VerifyObstructionStatus() switch - default case");
					//impossible
					break;
				}
			}//end switch

//			Debug.Log("box " + this.m_Index + " origin: " + origin.x + ", " + origin.y + ", " + origin.z);

			foreach(RaycastHit hit in Physics.RaycastAll(origin, Vector3.up, this.m_HeightOfObstructionCheck))
			{
				if (hit.collider.gameObject.layer == UnityEngine.LayerMask.NameToLayer("Obstructable")) {
//					Debug.Log ("Obstructable detected");
					this.m_IsObstructed = true;
					return;
				}
			}
		}
	}

	public bool IsGridBoxObstructed()
	{
		return this.m_IsObstructed;
	}

	/**A function to assign a single grid box its respective neighbors, taking into account its index and the number of boxes per row and column, respectively*/
	public void InitializeNeighbors(int box_index, int boxes_per_row, int boxes_per_column)
	{
		//While we're here, with the information we're given, initialize the box's index.
		this.m_Index = box_index;

		//if the box is simultaneously in all corners
		if (this.BoxIsInTopLeftCorner (box_index, boxes_per_row) && this.BoxIsInTopRightCorner (box_index, boxes_per_row)
		    && this.BoxIsInBottomLeftCorner (box_index, boxes_per_row, boxes_per_column) && this.BoxIsInBottomRightCorner (box_index, boxes_per_row, boxes_per_column)) 
		{
			#if TESTING_NEIGHBOR_INDICES
			Debug.Log ("Box " + box_index + " is simultaneously all corners");
			#endif
			this.AssignNeighbors (-1, -1, -1, -1);
			return;
		}

		//if the box is both in the top-left corner and the bottom-left corners...
		if (this.BoxIsInTopLeftCorner (box_index, boxes_per_row) && this.BoxIsInBottomLeftCorner(box_index, boxes_per_row, boxes_per_column)) {
			#if TESTING_NEIGHBOR_INDICES
			Debug.Log ("Box " + box_index + " is simultaneously Top left and bottom left corners");
			#endif
			this.AssignNeighbors (-1, box_index + 1, -1, -1);
			return;
		}
		//else if the box is both in the top-right and bottom-right corners...
		else if (this.BoxIsInTopRightCorner (box_index, boxes_per_row) && this.BoxIsInBottomRightCorner (box_index, boxes_per_row, boxes_per_column)) {
			#if TESTING_NEIGHBOR_INDICES
			Debug.Log ("Box " + box_index + " is simultaneously Top right and bottom right corners");
			#endif
			this.AssignNeighbors (-1, -1, -1, box_index - 1);
			return;
		}
		//else if the box is both in the top-left and top-right corners...
		else if (this.BoxIsInTopLeftCorner (box_index, boxes_per_row) && this.BoxIsInTopRightCorner (box_index, boxes_per_row)) {
			#if TESTING_NEIGHBOR_INDICES
			Debug.Log ("Box " + box_index + " is simultaneously Top left and top right corners");
			#endif
			this.AssignNeighbors (-1, -1, box_index + boxes_per_row, -1);
			return;
		}
		//else if the box is both in the bottom-left and bottom-right corners...
		else if (this.BoxIsInBottomLeftCorner(box_index, boxes_per_row, boxes_per_column) && this.BoxIsInBottomRightCorner(box_index, boxes_per_row, boxes_per_column))
		{
			#if TESTING_NEIGHBOR_INDICES
			Debug.Log ("Box " + box_index + " is simultaneously bottom left and bottom right corners");
			#endif
			this.AssignNeighbors (box_index - boxes_per_row, -1, -1, -1);
			return;
		}

		//if the box is both in the top row and the leftmost column...
		if (this.BoxIsInTopLeftCorner (box_index, boxes_per_row)) {
			//...then it has no top or left neighbor
			#if TESTING_NEIGHBOR_INDICES
			Debug.Log ("Box " + box_index + " is top left corner");
			#endif
			this.AssignNeighbors (-1, box_index + 1, box_index + boxes_per_row, -1);
			return;
		} 
		//...else if the box is in the top right corner of the grid...
		else if (this.BoxIsInTopRightCorner (box_index, boxes_per_row)) {
			//...then it has neither a top nor a right neighbor...
			#if TESTING_NEIGHBOR_INDICES
			Debug.Log ("Box " + box_index + " is top right corner");
			#endif
			this.AssignNeighbors (-1, -1, box_index + boxes_per_row, box_index - 1);
			return;
		} 
		//...else if the box is in the bottom left corner of the grid...
		else if (this.BoxIsInBottomLeftCorner (box_index, boxes_per_row, boxes_per_column)) {
			//...then it has neither a left nor a bottom neighbor
			#if TESTING_NEIGHBOR_INDICES
			Debug.Log ("Box " + box_index + " is bottom left corner");
			#endif
			this.AssignNeighbors (box_index - boxes_per_row, box_index + 1, -1, -1);
			return;
		} 
		//...else if the box is in the bottom right corner of the grid...
		else if (this.BoxIsInBottomRightCorner (box_index, boxes_per_row, boxes_per_column)) {
			//...then it has neither a right nor a bottom neighbor
			#if TESTING_NEIGHBOR_INDICES
			Debug.Log ("Box " + box_index + " is bottom right corner");
			#endif
			this.AssignNeighbors (box_index - boxes_per_row, -1, -1, box_index - 1);
			return;
		}


		//if the box is just in the top row...
		if (this.BoxIsInTopRow(box_index, boxes_per_row)) {
			//...then it has no top neighbor
			this.AssignNeighbors (-1, box_index + 1, box_index + boxes_per_row, box_index - 1);
		}
		//else if the box is just in the leftmost column...
		else if (this.BoxIsInLeftmostColumn(box_index, boxes_per_row)) {
			//...then it has no left neighbor
			this.AssignNeighbors (box_index - boxes_per_row, box_index + 1, box_index + boxes_per_row, -1);
		} 
		//else if the box is just in the rightmost column...
		else if (this.BoxIsInRightmostColumn(box_index, boxes_per_row)) {
			//...then it has no right neighbor
			this.AssignNeighbors (box_index - boxes_per_row, -1, box_index + boxes_per_row, box_index - 1);
		}
		//else if the box is just in the bottom row...
		else if (this.BoxIsInBottomRow(box_index, boxes_per_row, boxes_per_column)) {
			//...then it has no bottom neighbor
			this.AssignNeighbors (box_index - boxes_per_row, box_index + 1, -1, box_index - 1);
		} 
		//else if the box is not a special case
		else 
		{
			//then the box has all four neighbors
			this.AssignNeighbors (box_index - boxes_per_row, box_index + 1, box_index + boxes_per_row, box_index - 1);
		}
	}

	/**Helper function to assign the respective neighbors of the grid box*/
	private void AssignNeighbors(int top, int right, int bottom, int left)
	{
		//Top
		this.m_NeighborIndices.Add(top);
		//Right
		this.m_NeighborIndices.Add(right);
		//Bottom
		this.m_NeighborIndices.Add(bottom);
		//Left
		this.m_NeighborIndices.Add(left);
	}

	private bool BoxIsInTopRow(int box_index, int boxes_per_row)
	{
		return box_index < boxes_per_row;
	}

	private bool BoxIsInLeftmostColumn(int box_index, int boxes_per_row)
	{
		return box_index % boxes_per_row == 0;
	}

	private bool BoxIsInRightmostColumn(int box_index, int boxes_per_row)
	{
		return (box_index + 1) % boxes_per_row == 0;
	}

	private bool BoxIsInBottomRow(int box_index, int boxes_per_row, int boxes_per_column)
	{
		return (box_index + 1) > ((boxes_per_row * boxes_per_column) - boxes_per_row);
	}

	private bool BoxIsInTopLeftCorner(int box_index, int boxes_per_row)
	{
		return (this.BoxIsInLeftmostColumn (box_index, boxes_per_row) && this.BoxIsInTopRow (box_index, boxes_per_row));
	}

	private bool BoxIsInTopRightCorner(int box_index, int boxes_per_row)
	{
		return (this.BoxIsInTopRow (box_index, boxes_per_row) && this.BoxIsInRightmostColumn (box_index, boxes_per_row));
	}

	private bool BoxIsInBottomLeftCorner(int box_index, int boxes_per_row, int boxes_per_column)
	{
		return (this.BoxIsInBottomRow (box_index, boxes_per_row, boxes_per_column) && this.BoxIsInLeftmostColumn(box_index, boxes_per_row));
	}

	private bool BoxIsInBottomRightCorner(int box_index, int boxes_per_row, int boxes_per_column)
	{
		return (this.BoxIsInBottomRow (box_index, boxes_per_row, boxes_per_column) && this.BoxIsInRightmostColumn (box_index, boxes_per_row));
	}

	/**A function to help in debugging; prints a grid box's respective neighbors*/
	public void PrintNeighbors(int box_index)
	{
		string[] neighbor_names = System.Enum.GetNames (typeof(NEIGHBOR_POSITIONS));
		string message = "Neighboring indices for slot " + box_index + "; ";
		for (int index = 0; index < this.m_NeighborIndices.Count; index++) {
			message += neighbor_names[index] + ": " + this.m_NeighborIndices [index] + " ";
		}
		Debug.Log (message);
	}

	public int GetBoxIndex()
	{
		return this.m_Index;
	}

	public List<int> GetNeighborIndices()
	{
		return this.m_NeighborIndices;
	}

//	public bool SetGridboxHasBeenCheckedForPathfinding(bool status)
//	{
//		this.m_HasBeenCheckedForPathfinding = status;
//	}

	public void SetGridboxDistanceFromFlag(int distance)
	{
		this.m_DistanceFromDestination = 0;
	}

	/**A function to be called on each gridbox in our resetting of the grid after each successful pathfinding*/
	public void ResetGridboxDistanceFromFlag()
	{
		this.m_DistanceFromDestination = -1;
	}

	/**A function to tell us whether or not we've already checked the given slot in our search for the best path to take to reach the destination*/
	public bool GridboxHasBeenCheckedForPathfinding()
	{
		return this.m_DistanceFromDestination != -1;
	}
}
