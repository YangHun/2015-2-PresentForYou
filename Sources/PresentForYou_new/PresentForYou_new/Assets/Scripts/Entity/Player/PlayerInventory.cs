using UnityEngine;
using System.Collections;

public class PlayerInventory : MonoBehaviour {

	// NumberOfSnowCrystals		외부에서 얼음꽃 개수를 알기 위한 접근자.
	// snowCrystals				보유한 얼음꽃 개수.
	public int NumberOfSnowCrystals { get { return snowCrystals; } }
    public void init_Crystals() {
        snowCrystals = 0;
    }
	private int snowCrystals = 0;

	// 얼음꽃 개수를 number만큼 증가/감소시키는 함수들.
	public void GetSnowCrystal(int number) { snowCrystals += number; }
	public void LoseSnowCrystal(int number) { 
		snowCrystals -= number;
		// 얼음꽃 개수는 0 아래로 내려갈 수 없다.
		if (snowCrystals < 0) {
			Debug.LogError ("The number of Snow Crystal is negative.");
		}
	}
}
