using UnityEngine;
using System.Collections;

public class pfyTree : MonoBehaviour {
	
	public Tile tile {

		get {
			return tile;
		}
		set {
			tile = value;
			SetTexture ();
		}
	}

	[SerializeField]
	Sprite[] Textures;

	bool[] crystals;

	void Awake(){
		SetTexture ();
	}

	void SetTexture(){

		if (Textures.Length > 0) {

			int r =Random.Range (0, 10) % 2;
		//	Debug.Log (r);

			GetComponent<SpriteRenderer>().sprite = Textures[r];

			//gameObject.AddComponent<PolygonCollider2D>();
		}
	}
}
