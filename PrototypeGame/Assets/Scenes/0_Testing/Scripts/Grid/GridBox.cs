using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBox : MonoBehaviour {

	private enum NEIGHBOR_POSITIONS {TOP = 0, RIGHT = 1, BOTTOM = 2, LEFT = 3};
	private List<int> m_NeighborIndices = new List<int>();

	/**A function to assign a single grid box its respective neighbors, taking into account its index and the number of boxes per row and column, respectively*/
	public void InitializeNeighbors(int box_index, int boxes_per_row, int boxes_per_column)
	{
		/*
		 * Special cases:
		 * - Edges
		 * 		- Top row [0, ...], excluding
		 * 		- Bottom row [boxes_per_row - 1, ...]
		 * 		- Left edge [..., 0]
		 * 		- Right edge [..., boxes_per_column - 1]
		 * - Four corners
		 * 		- Top left; box_index = 0; no left or top neighbor
		 * 		- Top right; box_index = boxes_per_column - 1; no top or right neighbor
		 * 		- Bottom left; box_index = boxes_per_row*(boxes_per_column - 1); no bottom or left neighbor
		 * 		- Bottom right; box_index = (boxes_per_row*boxes_per_column) - 1; no bottom or right neighbor
		 * 
		 * General case:
		 * 	- Top neighbor: box_index - boxes_per_row
		 * 	- Right neighbor: box_index + 1
		 * 	- Bottom neighbor: box_index + boxes_per_row
		 * 	- Left neighbor: box_index - 1
		 * */

		//if the box is both in the top row and the leftmost column...
		if (this.BoxIsInTopLeftCorner (box_index, boxes_per_row)) {
			//...then it has no top or left neighbor
			this.AssignNeighbors (-1, box_index + 1, box_index + boxes_per_row, -1);
			return;
		} 
		//...else if the box is in the top right corner of the grid...
		else if (this.BoxIsInTopRightCorner (box_index, boxes_per_row)) {
			//...then it has neither a top nor a right neighbor...
			this.AssignNeighbors (-1, -1, box_index + boxes_per_row, box_index - 1);
			return;
		} 
		//...else if the box is in the bottom left corner of the grid...
		else if (this.BoxIsInBottomLeftCorner (box_index, boxes_per_row, boxes_per_column)) {
			//...then it has neither a left nor a bottom neighbor
			this.AssignNeighbors (box_index - boxes_per_row, box_index + 1, -1, -1);
			return;
		} 
		//...else if the box is in the bottom right corner of the grid...
		else if (this.BoxIsInBottomRightCorner (box_index, boxes_per_row, boxes_per_column)) {
			//...then it has neither a right nor a bottom neighbor
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
		return (box_index + 1) >= ((boxes_per_row * boxes_per_column) - boxes_per_row);
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

	public void PrintNeighbors(int box_index)
	{
		string[] neighbor_names = System.Enum.GetNames (typeof(NEIGHBOR_POSITIONS));
		string message = "Neighboring indices for slot " + box_index + "; ";
		for (int index = 0; index < this.m_NeighborIndices.Count; index++) {
			message += neighbor_names[index] + ": " + this.m_NeighborIndices [index] + " ";
		}
		Debug.Log (message);
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
