using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


class pfyStageEditor : EditorWindow {

	//Datas for Serialization
	int column = 1;
	int row = 1;
	bool[,] holes = null;
	bool[,] trees = null;
	bool[,] portals = null;

	UnityEngine.Object Tile = null;
	UnityEngine.Object tree = null;

	List< List<GameObject> > Map = null;
	GameObject map = null;
	
	int targetcolumn = 1;
	int targetrow = 1;

	bool enableMakeHole = false;
	bool enableMakeTree = false;
	bool enableMakePortals = false;

	Vector2 startingPoint = Vector2.zero;

	Vector2 Scrollpos; //에디터 스크롤링

	[MenuItem ("Present For You/World")]
	public static void ShowWindow(){

		// Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow (typeof(pfyStageEditor));

	}

	void OnGUI(){
	
		map = GameObject.Find ("Map");
	
		if (map == null) {

			GUILayout.Label ("Map Size", EditorStyles.boldLabel);
			GUILayout.Label ("지정된 크기로 맵을 만듭니다!", EditorStyles.label);

			column = EditorGUILayout.IntField ("가로", column);
			row = EditorGUILayout.IntField ("세로", row);
			startingPoint = EditorGUILayout.Vector2Field ("시작위치", startingPoint);
			Tile = EditorGUILayout.ObjectField ("Tile", Tile, typeof(UnityEngine.Object), false);


			if (GUILayout.Button ("make!")) {

				if (row * column > 0) {

					targetrow = row;
					targetcolumn = column;

					MakeMap (targetrow, targetcolumn);

				} else {
					Debug.Log ("0 이하의 값으론 맵을 만들 수 없어요 :(");
				}
			}

		} else {

			//init
			Map m = map.GetComponent<Map>();
			targetrow = m.row;
			targetcolumn = m.column;
			holes = m.holes;
			trees = m.trees;
			portals = m.portals;

			Scrollpos = EditorGUILayout.BeginScrollView (Scrollpos);
			GUILayout.Label ("이미 만들어진 맵이 있습니다!", EditorStyles.label);

			
			GUILayout.Label ("Map Setting", EditorStyles.boldLabel);
			GUILayout.Label ("1. 구멍내기\n : 플레이어가 갈 수 없는 곳을 설정합니다.", EditorStyles.label);
			
			if (map != null && map.transform.childCount > 0) {
				MakeToggles (targetrow, targetcolumn);
			} else
				GUILayout.Label ("맵을 다시 만들어주세요.", EditorStyles.label);
			
			
			GUILayout.Label ("\n2. 포탈설정\n : portal을 설정합니다.", EditorStyles.label);
			
			if (map != null && map.transform.childCount > 0)
				MakePortalToggles (targetrow, targetcolumn);
			else
				GUILayout.Label ("맵을 다시 만들어주세요.", EditorStyles.label);
			
			
			GUILayout.Label ("\n3. 나무심기\n : Tile 위에 나무를 심습니다", EditorStyles.label);
			
			if (map != null && map.transform.childCount > 0)
				MakeTreeToggles (targetrow, targetcolumn);
			else
				GUILayout.Label ("맵을 다시 만들어주세요.", EditorStyles.label);
			tree = EditorGUILayout.ObjectField ("Tree", tree, typeof(UnityEngine.Object), false);

			GUILayout.Label ("Save", EditorStyles.boldLabel);
			map = (GameObject)EditorGUILayout.ObjectField ("target Map", (UnityEngine.Object)map, typeof(GameObject), true);
			
			if (GUILayout.Button ("프리팹으로 저장")) {

                m.SaveField();

				string path = "Assets/Prefabs/Maps/Dummy/map.prefab";
				UnityEngine.Object prefab = PrefabUtility.CreateEmptyPrefab (path);
				PrefabUtility.ReplacePrefab (map, prefab, ReplacePrefabOptions.ConnectToPrefab);
				
			}

			GUILayout.Label ("Clear", EditorStyles.boldLabel);
			if (GUILayout.Button ("지우기")) {
				
				DestroyMap ();
				DestroyImmediate (GameObject.Find ("Map"));
			}
			EditorGUILayout.EndScrollView ();
		}
	}
	//}

	public void MakeMap(int r, int c){

		//Set Parent Object
		if (GameObject.Find ("Map") == null ) {
			
			map = new GameObject();
			map.name = "Map";
            map.tag = "Map";

		} else {
			map = GameObject.Find ("Map");
		}

		if (Map != null) {
			DestroyMap ();
		}
		if (Map == null) Map = new List< List<GameObject>> ();

		AddTiles (r,c);
		holes = new bool[r, c];
        trees = new bool[r, c];
		portals = new bool[r, c];

        Map m = map.AddComponent<Map>();
		m.column = c;
		m.row = r;
        m.holes = holes;
        m.trees = trees;
        m.portals = portals;
        m.startingPoint = startingPoint;

        map.AddComponent<PathGenerator>();
	}
	
	void AddTiles(int r, int c){

		for( int i = 0 ; i < r ; i ++) {

			List<GameObject> _map = new List<GameObject> ();

			for (int j = 0; j < c; j++) {
				GameObject obj = (GameObject)Instantiate( Tile , startingPoint, Quaternion.identity);
				obj.transform.position = setPosition (obj,i,j);
				obj.name = "Tile_"+i+"_"+j;
				obj.transform.SetParent(map.transform);

				obj.GetComponent<Tile>().SetOrderInLayer(i);
				obj.GetComponent<Tile>().map = map.GetComponent<Map>();

				_map.Add (obj);
			}

			Map.Add (_map);
		}
	}

	Vector2 setPosition(GameObject obj, int row, int column){

		Vector2 _pos = Vector2.zero;

		Texture _tex;

		//TODO : Texture 크기를 알기 위해 로드합니다 ; resource가 바뀌면 바꿔야 함
		if (_tex = obj.GetComponent<Tile>().TileTexture[0].texture ) {

			Vector3 size = new Vector3 (_tex.width/100f, _tex.height/100f, 0);
			// 100f : pixel resolution
			// length / resolution ==> world size

			if( row % 2 == 0 ) {

				_pos.x = startingPoint.x + (size.x * column);
				_pos.y = startingPoint.y + (size.y * row/2);

			}
			else{
				_pos.x = startingPoint.x + (size.x * column + (size.x/2));
				_pos.y = startingPoint.y + (size.y * row/2);

			}


		} else {
			_pos = startingPoint;
			Debug.Log ("Fail : load texture");
		}

		_pos.y *= (-1f);

		return _pos;
	}

	void DestroyMap(){

		foreach (List<GameObject> l in Map) {
			l.Clear();
		}
		Map.Clear ();

		GameObject[] objs = GameObject.FindGameObjectsWithTag ("Tile");
		foreach (GameObject obj in objs) {
			DestroyImmediate(obj, false);
		}
	}

	void MakeToggles(int _r, int _c){

		int cnt = 0;

		for (int i = 0; i < _r; i ++) {

			//EditorGUILayout.BeginToggleGroup(
			GUILayout.BeginHorizontal ();	

			for (int j = 0; j < _c; j ++) {

				holes[i, j] = EditorGUILayout.Toggle (holes[i, j]);
				if (holes[i, j]) {
					cnt ++;
				}
			}

			GUILayout.EndHorizontal ();
			//EditorGUILayout.EndToggleGroup ();

		}

		if (cnt > 0)
			enableMakeHole = true;
		else
			enableMakeHole = false;

		if (enableMakeHole) {
			if (GUILayout.Button ("Make Hole(s)!")) {

				Debug.Log ("Make Hole(s) ...");

				for (int i = 0; i < targetrow; i ++) {
					for (int j = 0; j < targetcolumn; j ++) {

						if (holes[i, j]) {

							Map [i] [j].GetComponent<Tile> ().type = global::Tile.tileType.cantwalk;
					
						} else {

							Map [i] [j].GetComponent<Tile> ().type = global::Tile.tileType.walkable;
						}

					}
				}
		
				Debug.Log ("Done!");

				map.GetComponent<Map>().PrintArrays();


			}
		}
	}

	void MakePortalToggles(int r, int c){
		
		if (holes != null && portals != null) {

			for (int i = 0; i < r; i ++) {
			
				GUILayout.BeginHorizontal ();	
			
				for (int j = 0; j < c; j ++) {
				
					if (!holes[i, j]) {
						portals[i, j] = EditorGUILayout.Toggle (portals[i, j]);
					} else {
						portals[i, j] = EditorGUILayout.Toggle (false);			
					}
				}
			
				GUILayout.EndHorizontal ();
			
			}
		
			if (GUILayout.Button ("Set Portal")) {
				
				Debug.Log ("Set Portal...");
				
				//TODO : set type of each tile "cantwalk"
				for (int i = 0; i < targetrow; i ++) {
					for (int j = 0; j < targetcolumn; j ++) {
						if (portals[i, j]) {

							Map [i] [j].GetComponent<Tile> ().type = global::Tile.tileType.portal;
							Debug.Log (Map [i] [j].name + "'s type = " + Map [i] [j].GetComponent<Tile> ().type);

						} else {

							if (holes[i, j])
								Map [i] [j].GetComponent<Tile> ().type = global::Tile.tileType.cantwalk;
							else
								Map [i] [j].GetComponent<Tile> ().type = global::Tile.tileType.walkable;
						}
					}
				}
				
				Debug.Log ("Done!");
			}
		}
	}
		


	void MakeTreeToggles(int r, int c){

		int cnt = 0;

		if(holes!= null && portals!= null & trees != null){
		
			for (int i = 0; i < r; i ++) {

				GUILayout.BeginHorizontal ();	
				
				for (int j = 0; j < c; j ++) {

					if( !holes[i, j] && !portals[i, j]){
						trees[i, j] = EditorGUILayout.Toggle (trees[i, j]);
					}
					else{
						trees[i, j] = EditorGUILayout.Toggle ( false );			
					}

					if (trees[i, j]) {
						cnt ++;
					}
				}
				
				GUILayout.EndHorizontal();

			}
			
			if (cnt > 0)
				enableMakeTree = true;
			else
				enableMakeTree = false;
			
			if (enableMakeTree) {
				if( GUILayout.Button ("Make Tree(s)!")) {
					
					Debug.Log ("Make Tree(s) ...");
					
					//TODO : set type of each tile "cantwalk"
					for( int i = 0 ; i < targetrow ; i ++){
						for( int j = 0 ; j < targetcolumn ; j ++){
							if( trees[i, j]){
								GameObject t = (GameObject)Instantiate(tree,SetTreePosition(Map[i][j].transform.position),Quaternion.identity);
								t.name = "Tree";
								t.transform.SetParent( Map[i][j].transform);
								t.GetComponent<SpriteRenderer>().sortingOrder = i;
								Map[i][j].GetComponent<Tile>().tree = t.GetComponent<pfyTree>();
							}
							else{

								try{
									Map[i][j].GetComponent<Tile>().tree = null;
								}
								catch(Exception ex){
									Debug.LogError ("Exception :: "+ex);
								}

							}
						}
					}
					
					Debug.Log ("Done!");
				}
			}
		}
	}

	Vector3 SetTreePosition(Vector3 pos){

        return pos;
        /*
		//TODO : Texture 크기를 알기 위해 로드합니다 ; resource가 바뀌면 바꿔야 함
		Texture t = (Texture)Resources.Load ("Dummy/tree", typeof(Texture));

		float width = 0f;
		float height = (t.height / (200f)); //tree의 길이는 tile의 2 배

		Vector3 size = Vector2.zero;
		size.x = width;
		size.y = height;

		return size + pos;
        */
	}




}
