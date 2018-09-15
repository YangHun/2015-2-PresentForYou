using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using MapIndex;

public class Map : MonoBehaviour {
	
    public int row;
    public int column;
    public Vector3 startingPoint;

	[SerializeField]
	private int[] TreeIce;
	[SerializeField]
	private int GroundIce;
	[SerializeField]
	private int Bears;

    [SerializeField]
    List<Index> hole = new List<Index>();
    [SerializeField]
    List<Index> tree = new List<Index>();
    [SerializeField]
    List<Index> portal = new List<Index>();
	[SerializeField]
	List<Index> crystal = new List<Index>();

	[SerializeField]
	Vector3[] CrystalPositions = new Vector3[5];

    public bool[,] holes;
    public bool[,] trees;
    public bool[,] portals;
	
    public void SaveField() {
        for (int i = 0; i < row; i++)
            for (int j = 0; j < column; j++) {
                if (holes[i, j])
                    hole.Add(new Index(i, j));
                if (trees[i, j])
                    tree.Add(new Index(i, j));
                if (portals[i, j])
                    portal.Add(new Index(i, j));
            }
    }

    public void LoadField() {
        holes = new bool[row, column];
        trees = new bool[row, column];
        portals = new bool[row, column];

        foreach (Index idx in hole)
            holes[idx.row, idx.col] = true;
        foreach (Index idx in tree)
            trees[idx.row, idx.col] = true;
        foreach (Index idx in portal)
            portals[idx.row, idx.col] = true;
    }

    public void PrintArrays() {
        for (int i = 0; i < row; i++)
            for (int j = 0; j < column; j++)
                Debug.Log("holes " + i + ", " + j + " = " + holes[i, j]);
        for (int i = 0; i < row; i++)
            for (int j = 0; j < column; j++)
                Debug.Log("holes " + i + ", " + j + " = " + trees[i, j]);
        for (int i = 0; i < row; i++)
            for (int j = 0; j < column; j++)
                Debug.Log("holes " + i + ", " + j + " = " + portals[i, j]);
	}	

	public void generateCrystals(){

		// 1. generate on the ground
		int n = GroundIce;
		UnityEngine.Object obj = Resources.Load ("Dummy/SnowCrystal");
		int[] N = TreeIce;
	//	N = RandomizeArray (N);

		if (obj == null) {
			Debug.LogError ("Ex ; Resource not loaded");
			return;
		}

		for (int i = row-1; i >=0; i--) {
			for (int j = column-1; j >= 0; j --) {
				Debug.Log ("Enter!");
				if ( (! chkPortal (i,j) )&& (! chkHole (i, j)) ) {
					Debug.Log ("Enter!");
					int r = UnityEngine.Random.Range (0, 2);
					//StartCoroutine ( instantiateCrystal(r, obj, i, j));
					instantiateCrystal (r, obj, i, j, false);
					n -= r;
				}

				if (n <= 0) {
					Debug.Log ("End Generating on the Ground");
					break;
				}
			}
		}

		//2. generate on the tree




		for (int i = 0; i < N.Length; i++) {

			instantiateCrystal (N [i], obj, tree [i].row, tree [i].col, true);

		}

		Debug.Log ("End Generating on the Tree");

	}

	void instantiateCrystal(int value, UnityEngine.Object obj, int row, int column, bool isTree){

		if (!isTree) {
			for (int k=0; k<value; k++) {

				Transform t = transform.FindChild ("Tile_" + row + "_" + column);
				if (t == null){
				
					Debug.LogError ("Cannot Find Tile");
					break;
				}
				else {
					GameObject ice = (GameObject)Instantiate (obj, t.position, Quaternion.identity);
					ice.name = k.ToString ();
					ice.transform.SetParent (t.transform);
					ice.transform.position = SetRandPos( t.position);
					ice.GetComponent<pfySnowCrystal>().tile = t.gameObject.GetComponent<Tile>();
				}
			}
		} else {

			int r = UnityEngine.Random.Range(0,4);

			for (int k=0; k<value; k++) {
				
				Transform t = transform.FindChild ("Tile_" + row + "_" + column).FindChild ("Tree");
				if (t == null){
					Debug.LogError ("Cannot Find Tree");
					break;
				}
				else {

					Vector3 pos = CrystalPositions[ ( r + k)/ 5 ];

					GameObject ice = (GameObject)Instantiate (obj, t.position + pos, Quaternion.identity);
					ice.name = k.ToString ();
					ice.transform.SetParent (t.transform);
					ice.GetComponent<pfySnowCrystal>().tree = t.gameObject.GetComponent<pfyTree>();
					ice.GetComponent<SpriteRenderer>().sortingOrder = t.gameObject.GetComponent<SpriteRenderer>().sortingOrder + 1;
				}
			}
		}
	}

	int[] RandomizeArray(int[] arr){

		int a = arr.Length;
		
		int[] result = new int[a];
		int[] chkn = new int[a];
		bool[] chk = new bool[a];

		for (int i = 0; i < a; i ++) {
			result[i] = 0;
			chk[i] = false;
		}

		int n = 0;
		int r;


		for (int i = 0; i < a; i ++) {

			r = UnityEngine.Random.Range(0,a-1);
			Debug.Log (r);
			bool check = true;

			for(int j = 0 ; j < a ; j ++){
				if( chk[r] == true ){
					check = false;
				}
			}
			Debug.Log (check);
			Debug.Log (i);
			if(check){
			
				chkn[i] = r;
				chk[i] = true;
			}
			else
			{
				i--;
			}
		}



		for (int i = 0; i < a; i ++) {

			Debug.Log (chkn[i] / arr[chkn[i]]);

			result [i] = arr[ chkn[i]];
		}

		return result;
	}

	bool chkHole(int row, int column){

		if (hole.Exists ( _i => (_i.row == row && _i.col == column ))) {
			return true;
		}
		else {
			return false;
		}
	}

	bool chkPortal(int row, int column){
		
		if (portal.Exists ( _i => (_i.row == row && _i.col == column ))) {
			return true;
		}
		else {
			return false;
		}
	}

	Vector2 SetRandPos ( Vector3 center ){

		int width = 420;
		int height = 280;

		float x, y;

		x = UnityEngine.Random.Range (1, 419);
		x -= 210;
		if (x > 0) {

			float y1 = ( x * 2 / 3) - 140f;
			float y2 = y1 + 140;

			y = UnityEngine.Random.Range ( (int)y1 , (int) y2);

		} else {

			float y1 =  (-1) * ( x * 2 / 3 ) - 140f;
			float y2 = y1 + 140;

			y = UnityEngine.Random.Range ( (int)y1, (int) y2);

		}

		if( y < 140 ) y+= 5f;

		Vector3 result = new Vector3 (x, y, 0f);

		result /= 100f;

		return result + center;

	}

	bool chkTree(int row, int column){
		if (tree.Exists ( _i => (_i.row == row && _i.col == column ))) {
			return true;
		}
		else {
			return false;
		}
	}
}
