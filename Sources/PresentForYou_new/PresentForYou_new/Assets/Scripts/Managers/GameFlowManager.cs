using UnityEngine;
using System.Collections;


/// <summary>
/// 게임의 플로우를 관리하는 매니저.
/// </summary>
public class GameFlowManager : MonoBehaviour {

    //세이브 필요한 변수들 : see_end, back_home, last_stage, d-day, save_state, stage_data(이건 1,2,3,4스테이지 구분하는 놈인데, 스테이지를 통째로 저장하게되면 필요 없어요)
    //이어서 : cold_air, PlayerInventory의 snowCrystal, Player state의 coldair
    //리소스매니져의 snowcrystal,coldair, 플레이어가 가진 snowcrystal,coldair

    private static GameFlowManager _instance = null; //초기화 전까진 null 상태
	public static GameFlowManager I { get { return _instance; } }

	//private enum STATE : 게임플로우매니저의 스테이트 enum 값
	//none : 기본 값.
	public enum STATE{

		none,
		st_App_Start,
		st_Title,
		st_Reminiscence,
		st_Tutorial,
        st_Home,
        st_Select,
        st_Stage,
        st_Ending
	}

	private STATE Current_State;
	private STATE Next_State = STATE.none;
	private bool FirstFrame = true; //첫 시작이면 첫 프레임이니까 true;

	// Use this for initialization
	void Start () {
	

		Current_State = STATE.st_App_Start; //시작 스테이트 지정

		//singleton Init
		if (_instance != null) { //만약 이미 static 변수가 존재한다면

			Destroy (this.gameObject); // 이 스크립트가 붙은 게임오브젝트를 Destroy

		} else { //만약 static 변수가 존재하지 않는다면, 즉, 내가 처음 만들어진거라면

			_instance = this; //_instance가 가리키는 값은 나

			DontDestroyOnLoad(this.gameObject); //그리고 나는 씬이 옮겨져도 지워지면 안됨.

			if(transform.parent != null ){ //만약 상위 계층이 있다면
				DontDestroyOnLoad( transform.parent.gameObject); // parent도 지우지 마라
			}
		}
        
    }

	// Update is called once per frame
	void Update () {
	
		switch (Current_State) {

		case STATE.none : // Do nothing
			break;
		case STATE.st_App_Start :
			Funct_AppStart(Time.time);
			break;
		case STATE.st_Title :
			Funct_Title(Time.time);
			break;
		case STATE.st_Reminiscence:
			Funct_Reminiscence(Time.time);
			break;
        case STATE.st_Tutorial:
            Funct_Tuto(Time.time);
            break;
        case STATE.st_Home:
            Funct_Home(Time.time);
            break;
        case STATE.st_Select:
            Funct_Select(Time.time);
            break;
        case STATE.st_Stage:
            Funct_Stage(Time.time);
            break;
        case STATE.st_Ending:
            Funct_Ending(Time.time);
            break;
		}

		if (FirstFrame) {
			FirstFrame = false;
		}
	}

	void LateUpdate(){ //Update가 끝나면 실행된다

		if (Next_State != STATE.none) {

			Current_State = Next_State; //다음 frame의 update 때 바뀐 State의 함수가 실행된다
			Next_State = STATE.none;
			FirstFrame = true;
		} else {
			FirstFrame = false;
		}
	}



    public bool see_end = false;    //엔딩 이후 최초시작 결정
    bool back_home = false; //집으로 2회 진입 파악
    public int last_stage;  //스테이지 이동시 날짜 +를 위해 이전 스테이지 저장(warp함수에서 저장됨)
    public string save_state="none";
    public int stage_data=0;  //1,2,3,4스테이지 파악용. 0은 집


    //public int d_day;  //디데이!
    public double cold_air;




	void Funct_AppStart(float dt){
		if (FirstFrame) {
            ResourceManager.I.init_resource();


		}
	
		Debug.Log ("State AppStart!");

        //TODO: 처리 : 게임을 초기화하고, save file을 불러온다. see_end, back_home값 대입.
        //save_state에 저장해 놨던 값을 대입한다. 세이브파일이 없으면 건드리지 않는다.

        //일단 Title State로 넘김 --> 시작 Scene이 Title
        ChangeState(STATE.st_Title);

	}

	void Funct_Title(float dt){
		if (FirstFrame) {


		}

		Debug.Log ("State Title!");

		//TODO : 처리 : 게임시작, 회상룸 진입, 게임종료등의 GUI제공 및 처리

	}

	void Funct_Reminiscence(float dt){
		if (FirstFrame) {
			
		}

		Debug.Log ("State 회상룸!");

		//TODO : 처리 : save file에서 보여줄 회상들을 찾아서 제공해준다
	}

    void Funct_Tuto(float dt){
        if (FirstFrame){

        }

        Debug.Log("State Tuto!");

        //TODO : 처리 : 게임설명, if(see_end==1) {skip 버튼 제공, see_end=0}

        if (see_end == true) {

            //skip버튼 지원

            see_end = false;
        }

        ChangeState(STATE.st_Home);
    }

    void Funct_Home(float dt) {
        if (FirstFrame) {
            last_stage = 0;
            //TODO :last_stage랑 state_data(집은 0)으로 시간경과 구해서 d-day 더함. 100 초과시 집으로 못들어간 삐침엔딩
        }
        Debug.Log("State Home!");
        //TODO : 처리 : 셀렉트 state나 엔딩 state로 transition

        if (back_home==false) {
            Application.LoadLevel("Select");
            ChangeState(STATE.st_Select);
            save_state = "Select";  //셀렉트화면으로 씬 이동 저장
            //d_day = 0;  //디데이 초기화
            
            cold_air = 0;
            back_home = true;
        }
        else {
            Application.LoadLevel("Ending");
            ChangeState(STATE.st_Ending);
        }
    }

    void Funct_Select(float dt) {
        if (FirstFrame) {
			UIManager.I.init_text();//GUI   
			UIManager.I.show_text();
        }

        /*DontDestroyOnLoad(GameObject.Find("D_day"));
        DontDestroyOnLoad(GameObject.Find("Cold_air"));
        DontDestroyOnLoad(GameObject.Find("Snow_flower"));
        DontDestroyOnLoad(GameObject.Find("Canvas"));*/

        Debug.Log("State Select!");
        //TODO : 처리 : 유저의 선택에 따라 스테이지 혹은 집으로 state를 transition
    }

    void Funct_Stage(float dt) {
        if (FirstFrame) {
            //TODO : d-day경과를 last_stage랑 stage_data로 알아내서 더해줍니당
            DateManager.I.AddDate(stage_data);
            EndingManager.I.stage_check[stage_data-1]=true;

			//TODO : 처리 : 받은 정보에 따라 Stage initiation, 게임오브젝트간 처리, 스테이지 탈출 처리
			WorldManager.I.StageInit (stage_data);
			UIManager.I.init_text();//GUI
        }

        Debug.Log("State Stage!");
        
        //일단 stage_data라는 변수에서 1,2,3,4스테이지는 알 수 있음
        if (DateManager.I.RemainDay < 0) {
            Application.LoadLevel("Home");
            ChangeState(STATE.st_Home);
        }
    }


    void Funct_Ending(float dt) {
        if (FirstFrame) {

        }

        Debug.Log("State Ending!!!");
        //TODO : 처리 : 결과에 맞는 엔딩 출력, 타이틀씬으로 이동, 엔딩 등재, (bool) see_end=1;


        see_end = true;
        Application.LoadLevel("Title");
        ChangeState(STATE.st_Title);

    }

    //System.Object는 enum의 상위 클래스입니다. enum type 넣으면 돼요.
    public void ChangeState( System.Object _next ){

		Next_State = (STATE) _next; //다음 스테이트를 지정한다. 이 스테이트는 다음 프레임에 바뀐다.

	}
}
