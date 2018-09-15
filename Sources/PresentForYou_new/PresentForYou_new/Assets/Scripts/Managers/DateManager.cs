using UnityEngine;
using System.Collections;

/// <summary>
/// 게임의 시스템인 '날짜'를 관리하는 매니저.
/// </summary>
public class DateManager : MonoBehaviour {
    #region SingleTone
    private static DateManager _instance = null; //초기화 전까진 null 상태
	public static DateManager I { get { return _instance; } }

	// Use this for initialization
	void Start () {
		//singleton Init
		if (_instance != null) { 
			
			Destroy (this.gameObject); 
			
		} else { 
			
			_instance = this;
			DontDestroyOnLoad(transform.root.gameObject);
		}
	}
    #endregion

    private const int d_day = 45;
    private int currentDate = 0;

    public int Dday { get { return d_day; } }

    public int RemainDay { get { return d_day - currentDate; } }

    public void AddDate (int stage) {
        int date=0;
        if(GameFlowManager.I.last_stage == 0) {
            switch (stage) {
                case 1:
                    date = 10;
                    break;
                case 2:
                    date = 20;
                    break;
                case 3:
                    date = 15;
                    break;
                case 4:
                    date = 15;
                    break;
            }
        }
        if (GameFlowManager.I.last_stage == 1) {
            switch (stage) {
                case 0:
                    date = 10;
                    break;
                case 2:
                    date = 16;
                    break;
                case 3:
                    date = 11;
                    break;
                case 4:
                    date = 19;
                    break;
            }
        }
        if (GameFlowManager.I.last_stage == 2) {
            switch (stage) {
                case 1:
                    date = 16;
                    break;
                case 3:
                    date = 9;
                    break;
                case 4:
                    date = 17;
                    break;
                case 0:
                    date = 20;
                    break;
            }
        }
        if (GameFlowManager.I.last_stage == 3) {
            switch (stage) {
                case 1:
                    date = 11;
                    break;
                case 2:
                    date = 9;
                    break;
                case 4:
                    date = 12;
                    break;
                case 0:
                    date = 15;
                    break;
            }
        }
        if (GameFlowManager.I.last_stage == 4) {
            switch (stage) {
                case 1:
                    date = 19;
                    break;
                case 2:
                    date = 17;
                    break;
                case 3:
                    date = 12;
                    break;
                case 0:
                    date = 15;
                    break;
            }
        }
        currentDate += date;

        if (currentDate >= d_day) {
            // 엔딩 호출
        }
    }
}
