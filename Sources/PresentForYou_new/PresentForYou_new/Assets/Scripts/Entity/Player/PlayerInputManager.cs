using UnityEngine;
using System.Collections.Generic;

// 키보드를 누르는 3가지 경우
// KeyTyped		키를 누르는 순간
// KeyPressed	키를 누르고 있는 동안
// KeyReleased	키를 떼는 순간
public enum KeyEventType {
	KeyTyped, KeyPressed, KeyReleased
}
// PlayerInputManager에서 사용할 delegate 함수.
public delegate void KeyEvent();

// 키 입력 이벤트를 등록하고 호출하는 관리자.
public class PlayerInputManager : MonoBehaviour {

	// KeyEventType enum에 등록된 이벤트들을 정수로 변환.
	// registeredKeys와 keyEvents에서 이벤트 종류에 따라 빠르게 탐색하기 위함.
	// NUMBER_OF_KEY_TYPE	키 입력 이벤트 종류
	// KEY_TYPED			키를 누르는 순간
	// KEY_PRESSED			키를 누르는 동안
	// KEY_RELEASED			키를 떼는 순간
	const int NUMBER_OF_KEY_TYPE = 3;
	const int KEY_TYPED = (int)KeyEventType.KeyTyped;
	const int KEY_PRESSED = (int)KeyEventType.KeyPressed;
	const int KEY_RELEASED = (int)KeyEventType.KeyReleased;

	// registeredKeys[KEY_EVENT_TYPE] = KEY_CODE
	// KEY_EVENT_TYPE에 KEY_CODE가 등록되어 있다.
	// e.g. registeredKeys[KEY_TYPED] = KeyCode.Space
	//		Space 키가 눌리는 순간 호출할 이벤트가 있다.
	//
	// keyEvents[KEY_EVENT_TYPE][KEY_CODE] = KEY_EVENT
	// e.g. keyEvents[KEY_TYPED][KeyCode.Space] = KeyEvent( MethodA ));
	//		Space 키가 눌리는 순간 MethodA를 호출해야 한다.
	//
	private List <KeyCode>[] registeredKeys;
	private Dictionary <KeyCode, KeyEvent>[] keyEvents;

	// 초기화.
	void Awake () {
		registeredKeys = new List<KeyCode>[NUMBER_OF_KEY_TYPE];
		keyEvents = new Dictionary<KeyCode, KeyEvent>[NUMBER_OF_KEY_TYPE];
		for (int i = 0; i < NUMBER_OF_KEY_TYPE; i ++) {
			registeredKeys[i] = new List<KeyCode>();
			keyEvents[i] = new Dictionary<KeyCode, KeyEvent>();
		}
	}
	
	// 매 프레임마다 호출.
	void Update () {
		// KEY_TYPED 이벤트에 호출하도록 등록된 이벤트가 있다면 호출.
		foreach (KeyCode key in registeredKeys[KEY_TYPED]) {
			if (Input.GetKeyDown (key))
				InvokeKeyEvent(key, KEY_TYPED);
		}
		// KEY_PRESSED 이벤트에 호출하도록 등록된 이벤트가 있다면 호출.
		foreach (KeyCode key in registeredKeys[KEY_PRESSED]) {
			if (Input.GetKey (key))
				InvokeKeyEvent(key, KEY_PRESSED);
		}
		// KEY_RELEASED 이벤트에 호출하도록 등록된 이벤트가 있다면 호출.
		foreach (KeyCode key in registeredKeys[KEY_RELEASED]) {
			if (Input.GetKeyUp (key))
				InvokeKeyEvent(key, KEY_RELEASED);
		}
	}

	#region Register Events
	// 키 입력 발생 시 콜백 함수를 호출하도록 keyEvents와 registeredKeys에 등록.
	public void RegisterKeyEvent (KeyCode key, KeyEvent keyEvent, int type) {
		if (keyEvents[type].ContainsKey (key))
			keyEvents[type][key] += keyEvent;
		else {
			if (registeredKeys[type].Contains(key) == false)
				registeredKeys[type].Add (key);
			keyEvents[type].Add (key, keyEvent);
		}
	}
	#endregion

	#region Invoke Events
	// KEY_TYPE과 KEY_CODE에 대응하는 함수를 호출.
	private void InvokeKeyEvent (KeyCode key, int type) {
		KeyEvent _event = null;
		if (keyEvents [type].TryGetValue (key, out _event)) {
			if (_event != null)
				_event ();
		}
	}
	#endregion
}