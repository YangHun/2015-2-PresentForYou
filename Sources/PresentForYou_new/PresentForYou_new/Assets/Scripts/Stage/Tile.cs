using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {
	
	public Map map;

	//for editor
	// TODO : change to Tree class
	private pfyTree _tree = null;
	public pfyTree tree { 
		get { return _tree; } 
		set{ 
			if( _tree != null){
				DestroyImmediate(_tree.gameObject);
			}
			_tree = value;
		}
	}

	private bool[] crystals;

	
	public Sprite[] TileTexture;
	public Sprite PortalTexture;
	
	public enum tileType { walkable, cantwalk, portal}
	
	[SerializeField]
	private tileType _type;
	
	public tileType type {
		
		get{
			return _type;
		}
		set{
			
			_type = value;
			
			
			switch( _type){
				
			case tileType.walkable :
				SetTexture (_type);
				gameObject.SetActive(true);
				break;
			case tileType.cantwalk :
				gameObject.SetActive(false);
				break;
			case tileType.portal :
				SetTexture(_type);
				break;
			}
		}
	}
	
	void SetTexture (tileType t){
		
		switch (t){
			
		case tileType.portal:
			GetComponent<SpriteRenderer>().sprite = PortalTexture;
			break;
			
		case tileType.walkable:
			// change the texture of tile randomly
			// get textures which are setted at Inspector
			// TODO : set textures at editor--> Rann's
			if (TileTexture.Length > 0) {
				int r = Random.Range (0, TileTexture.Length - 1);
				GetComponent<SpriteRenderer> ().sprite = TileTexture[r];
			}
			
			break;
		}
	}
	
	public void SetOrderInLayer( int n ){
		
		this.gameObject.GetComponent<SpriteRenderer>().sortingOrder = n;
		
	}
	
	void OnTriggerEnter2D(Collider2D col){
		if ( _type == tileType.portal && col.gameObject.tag == "Player") {
			WorldManager.I.Warp();
		}
		
		Renderer r = col.gameObject.GetComponent<SpriteRenderer> ();
		
		// TODO : object들의 Rendering order 값 보정
		if (r.sortingLayerName == "Object") {
			
			r.sortingOrder = GetComponent<SpriteRenderer>().sortingOrder;
		}
		
	}

	void GenerateCrystals(int n){

		if (crystals == null && n > 0) {

			crystals = new bool[n];
			Object obj = Resources.Load ("Dummy/SnowCrystal");

			for( int i = 0 ; i < n ; i++){
			GameObject ice = (GameObject)Instantiate (obj, transform.position, Quaternion.identity);
			ice.name = i.ToString ();
			ice.transform.SetParent (transform.transform);
			ice.transform.position = SetRandPos( transform.position);
			ice.GetComponent<pfySnowCrystal>().tile = this;
			}
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
}
