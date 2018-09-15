using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {


    private static UIManager _instance = null;
    public static UIManager I { get { return _instance; } }
    
    //Title Scene Functions
    // ????????
    Text show_d_day, show_snow_flower, show_cold_air;

    void Start() {
        if (_instance != null) {
            Destroy(this.gameObject);
        }
        else {
            _instance = this;

            DontDestroyOnLoad(this.gameObject);

            if (transform.parent!= null) {
                DontDestroyOnLoad(transform.parent.gameObject);
            }

        }

    }
	public void show_text(){
		if (show_d_day != null) {
			//GameFlowManager.I.cold_air = cold_air.coldAir;
			Debug.Log("printing text");
			show_d_day.text = "D-Day -" + DateManager.I.RemainDay;
			if(Application.loadedLevel ==2){
				show_cold_air.text = "Cold Air : " + (ResourceManager.I.cold_Air + cold_air.coldAir) + "%";
				show_snow_flower.text = "Snow Flower : " + (ResourceManager.I.snow_Crystal+inven.NumberOfSnowCrystals);
			}
			else if(Application.loadedLevel == 6){
				show_cold_air.text ="Cold Air : " + ResourceManager.I.cold_Air + "%";
				show_snow_flower.text = "Snow Flower : " + ResourceManager.I.snow_Crystal;
			}
		}
	}
	private PlayerStateManager cold_air;// = GameObject.Find("Player").GetComponent<PlayerStateManager>();
    public void init_text() {
		if (Application.loadedLevel == 2) { //2 : Stage scene
			inven = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerInventory> ();
			cold_air = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerStateManager> ();
		}

		show_snow_flower = GameObject.Find ("Snow_flower").GetComponent<Text> ();
		show_cold_air = GameObject.Find ("Cold_air").GetComponent<Text> ();
		show_d_day = GameObject.Find ("D_day").GetComponent<Text> ();

    }
    PlayerInventory inven;
    void Update() {
        if (Application.loadedLevel == 2) {
			GameFlowManager.I.cold_air =cold_air.coldAir;
			show_text();
		}
    }
    
    public void Game_Start() {
        if (GameFlowManager.I.see_end == true || GameFlowManager.I.save_state.Equals("none")) {  // +|| 세이브파일이 없을때
            Application.LoadLevel("Tutorial");
            GameFlowManager.I.ChangeState(GameFlowManager.STATE.st_Tutorial);
        }
        else {  //세이브가 있을때 && 엔딩을 보지 않았을때
            if (GameFlowManager.I.save_state.Equals("Select")) {//집에서, stage에서 select로 이동하는 경우에 Select 저장
                Application.LoadLevel("Select");
                GameFlowManager.I.ChangeState(GameFlowManager.STATE.st_Select);
            }
            else if (GameFlowManager.I.save_state.Equals("Stage")) {
                //else { 
                Application.LoadLevel("Stage");
                GameFlowManager.I.ChangeState(GameFlowManager.STATE.st_Stage);
            }
        }
    }

    public void Collection() {
        Application.LoadLevel("Collection");
        GameFlowManager.I.ChangeState(GameFlowManager.STATE.st_Reminiscence);
    }
    /*public void Title() {
        Application.LoadLevel("Title");
        GameFlowManager.I.ChangeState(GameFlowManager.STATE.st_Title);
    }*/

    public void End_Game() {
        Application.Quit();
    }

    public void Stage(Button a) {
        string b = a.gameObject.name;
        switch (b) {
            case "Stage1":
                if (GameFlowManager.I.last_stage != 1) {
                    GameFlowManager.I.save_state = "Stage";
                    GameFlowManager.I.stage_data = 1;
                    Application.LoadLevel("Stage");
                    GameFlowManager.I.ChangeState(GameFlowManager.STATE.st_Stage);
                }
                break;
            case "Stage2":
                if (GameFlowManager.I.last_stage != 2) {
                    GameFlowManager.I.save_state = "Stage";
                    GameFlowManager.I.stage_data = 2;
                    Application.LoadLevel("Stage");
                    GameFlowManager.I.ChangeState(GameFlowManager.STATE.st_Stage);
                }
                break;
            case "Stage3":
                if (GameFlowManager.I.last_stage != 3) {
                    GameFlowManager.I.save_state = "Stage";
                    GameFlowManager.I.stage_data = 3;
                    Application.LoadLevel("Stage");
                    GameFlowManager.I.ChangeState(GameFlowManager.STATE.st_Stage);
                }
                break;
            case "Stage4":
                if (GameFlowManager.I.last_stage != 4) {
                    GameFlowManager.I.save_state = "Stage";
                    GameFlowManager.I.stage_data = 4;
                    Application.LoadLevel("Stage");
                    GameFlowManager.I.ChangeState(GameFlowManager.STATE.st_Stage);
                }
                break;
            case "Home":
                GameFlowManager.I.stage_data = 0;
                GameFlowManager.I.ChangeState(GameFlowManager.STATE.st_Home);
                break;
            case "Title":
                Application.LoadLevel("Title");
                GameFlowManager.I.ChangeState(GameFlowManager.STATE.st_Title);
                break;
        }
    }
    
}
