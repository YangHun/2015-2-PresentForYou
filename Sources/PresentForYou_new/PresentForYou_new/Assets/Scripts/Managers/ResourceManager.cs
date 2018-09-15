using UnityEngine;
using System.Collections;


/// <summary>
/// 게임 내 Sound 등의 리소스를 관리하는 매니저.
/// </summary>
public class ResourceManager : MonoBehaviour {

	private static ResourceManager _instance = null; //초기화 전까진 null 상태
	public static ResourceManager I { get { return _instance; } }
    public void init_resource() {
        snow_Crystal = 0;
        cold_Air = 0;
    }
	// Use this for initialization
	void Start () {
		//singleton Init
		if (_instance != null) { 

			Destroy (this.gameObject); 
			
		} else { 
			
			_instance = this;

		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public int snow_Crystal;
    public float cold_Air;
}
