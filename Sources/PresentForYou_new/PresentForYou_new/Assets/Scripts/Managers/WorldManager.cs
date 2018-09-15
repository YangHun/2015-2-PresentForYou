using UnityEngine;
using System.Collections;

/// <summary>
/// 싱글톤 클래스 WorldManager
/// Scene 내부의 Object와 Object간 Event를 관리합니다.
/// </summary>
public class WorldManager : MonoBehaviour {

    private static WorldManager instance = null;
    public static WorldManager I {
        get { return instance; }
    }

    GameObject map = null;//current map in the game stage

	GameObject player = null;

    // Use this for initialization
    void Start() {



        // singleton Funtion
        if (instance != null) {
            Destroy(this.gameObject);
            return;
        }
        else {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    // Update is called once per frame
    void Update() {


    }

    /// <summary>
    /// Stage scene을 초기화한다.
    /// </summary>
    public void StageInit(int stagenum) {

        StartCoroutine (_StageInit (stagenum));
     
        
    }


    IEnumerator _StageInit(int stagenum) {

		LoadMap(stagenum);
		SetPlayer ();
		yield return new WaitForSeconds(Time.deltaTime);
		find_player ();	

		GenerateCrystals();

    }

    void LoadMap(int _stageNum) {

        Object _prefab = null;

        switch (_stageNum) {
            case 1: _prefab = Resources.Load("Map/dummy/1");
                break;
            case 2: _prefab = Resources.Load("Map/dummy/2");
                break;
            case 3: _prefab = Resources.Load("Map/dummy/3");
                break;
            case 4: _prefab = Resources.Load("Map/dummy/4");
                break;
            default:
                break;
        }

        if (_prefab != null) {
            map = (GameObject)Instantiate(_prefab, Vector3.zero, Quaternion.identity);
            map.name = "Map";
        } else {
            Debug.LogError("Error : Cannot find map prefab!");
            return;
        }
    }

	void SetPlayer(){

		player = (GameObject)Instantiate (Resources.Load ("In-Game/Player"), Vector3.zero, Quaternion.identity);
		
		//TODO : 플레이어의 초기 위치
		GameObject.Find ("Camera").transform.SetParent (player.transform);


		player.transform.position = map.GetComponent<Map> ().startingPoint + new Vector3(4.2f, -2.8f, 0f);

	}

    void GenerateCrystals() {

        if (map != null) {
            map.GetComponent<Map>().generateCrystals();

        } else {

            Debug.LogError("Error : current map gameobj is null!");
            return;
        }
    }
    PlayerInventory inven; PlayerStateManager cold_air;
    public void find_player() {
     //    inven = GameObject.Find("Player").GetComponent<PlayerInventory>();
		inven = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerInventory>();
//         cold_air = GameObject.Find("Player").GetComponent<PlayerStateManager>();
		cold_air = GameObject.FindGameObjectWithTag ("Player").GetComponent < PlayerStateManager> ();
    }   

	public void GetSnowCrystal(int n){

		player.GetComponent<PlayerInventory> ().GetSnowCrystal (n);
	}

	public void Warp(){
        // TODO : When Player walk onto 'portal', change Scene and Save Data

        // 1. save data.0
        GameFlowManager.I.last_stage = GameFlowManager.I.stage_data;
        GameFlowManager.I.save_state = "Select";//현 state 저장
        ResourceManager.I.snow_Crystal += inven.NumberOfSnowCrystals;
        if (ResourceManager.I.cold_Air + cold_air.coldAir<=100) {
            ResourceManager.I.cold_Air += cold_air.coldAir;
        }
        else {
            ResourceManager.I.cold_Air = 100;
        }
        inven.init_Crystals();
        cold_air.init_coldAir();
        //세이브 추가처리..(d-day)

        // 2. Change Scene
        Application.LoadLevel("Select");
        GameFlowManager.I.ChangeState(GameFlowManager.STATE.st_Select);

	}
}
