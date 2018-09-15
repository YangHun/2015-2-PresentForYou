using UnityEngine;
using System.Collections;

public class pfySnowCrystal : MonoBehaviour{

	public Tile tile;
	public pfyTree tree;


	[SerializeField]
	Sprite[] Textures;

	public void SetTexture(){

		if (Textures.Length > 0) {

			int r = Random.Range (0, Textures.Length - 1);
			GetComponent<SpriteRenderer>().sprite = Textures[r];

		}
	}

	void OnTriggerEnter2D( Collider2D col){

		if (col.gameObject.tag == "Player") {

			if ( (this.gameObject.transform.parent.tag == "Tree") && col.gameObject.GetComponent<PlayerStateManager>().State == PlayerState.ON_CLIMB){
				WorldManager.I.GetSnowCrystal (1);
				
				Destroy (this.gameObject);
				
			}
			else if ( this.gameObject.transform.parent.tag == "Tile"){
				WorldManager.I.GetSnowCrystal (1);
				
				Destroy (this.gameObject);

			}
		}
	}
}
