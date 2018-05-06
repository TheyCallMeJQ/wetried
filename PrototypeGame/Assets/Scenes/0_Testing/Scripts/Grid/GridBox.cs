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

		//else if not a special case
		//Top
		this.m_NeighborIndices.Add(box_index - boxes_per_row);
		//Right
		this.m_NeighborIndices.Add(box_index + 1);
		//Bottom
		this.m_NeighborIndices.Add(box_index + boxes_per_row);
		//Left
		this.m_NeighborIndices.Add(box_index - 1);
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
