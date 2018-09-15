using UnityEngine;
using System.Collections;

/// <summary>
/// 엔딩과 회상룸을 관리하는 매니저.
/// </summary>
public class EndingManager : MonoBehaviour {
    
    
	private static EndingManager _instance = null; //초기화 전까진 null 상태
	public static EndingManager I { get { return _instance; } }
    public bool[] stage_check = new bool[4];
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
    int true_end_check = 0;
    public void show_ending() {
        if (DateManager.I.RemainDay >= 0) {
            if (GameObject.Find("Player").GetComponent<PlayerInventory>().NumberOfSnowCrystals >check_snow_flower()) {
                if (GameFlowManager.I.cold_air>=100) {
                    switch (true_end_check) {
                        case 1:
                            //가루눈 엔딩
                            break;
                        case 2:
                            //진눈꺠비 엉엉
                            break;
                        case 4:
                            //싸락눈
                            break;
                        case 8:
                            //함박눈
                            break;
                        case 3:
                        case 5:
                        case 9:
                        case 6:
                        case 10:
                        case 12:
                            //싸락눈*2
                            break;
                        case 7:
                        case 11:
                        case 13:
                        case 14:
                        case 15:
                            //함박눈*2
                            break;
                    }
                }
                else {
                    //찬공기 부족, 얼음꽃 엔딩
                }
            }
            else {
                //얼음꽃 부족, 감기엔딩
            }
        }
        else {
            //삐짐엔딩
        }
    }
    

    int check_snow_flower(){
        int flower = 0;
        for(int fo=0; fo<4; fo++) {
            if (stage_check[fo] == true) {
                flower = flower + 25 + 5 * fo;
                true_end_check += (int)Mathf.Pow(2, fo);
            }
        }
        return flower;
    }


}
