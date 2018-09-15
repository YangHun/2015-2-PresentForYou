using UnityEngine;
using System.Collections;

/// <summary>
/// This class detects keyboard inputs and controls Player object.
/// </summary>
using System.Collections.Generic;


public class PlayerMove : MonoBehaviour {

	// VERTICAL_SPEED		상하 이동 속도.
	// HORIZONTAL_SPEED		좌우 이동 속도.
	// pressedKeys			현재 누르고 있는 방향키 개수.
	const float VERTICAL_SPEED = 3.0f;
	const float HORIZONTAL_SPEED = 5.0f;
	private byte pressedKeys = 0;

	private PlayerStateManager _stateManager;
    private PathGenerator pathGenerator;

	// 초기화.
	void Start () {
		const int KEY_TYPED = (int)KeyEventType.KeyTyped;
		const int KEY_PRESSED = (int)KeyEventType.KeyPressed;
		const int KEY_RELEASED = (int)KeyEventType.KeyReleased;

		PlayerInputManager _input = GetComponent <PlayerInputManager> ();
		_stateManager = GetComponent <PlayerStateManager> ();
 //      
		if (GameObject.FindGameObjectWithTag ("Map").GetComponent<PathGenerator> () != null) {

			pathGenerator = GameObject.FindGameObjectWithTag("Map").AddComponent<PathGenerator>();

		} else {
			pathGenerator = GameObject.FindGameObjectWithTag("Map").GetComponent<PathGenerator>();
		}

		keyList = new List <PlayerDirection>();

		// 방향키를 입력하면 플레이어 캐릭터가 이동.
		_input.RegisterKeyEvent (KeyCode.UpArrow, new KeyEvent (OnUpArrow), KEY_PRESSED);
		_input.RegisterKeyEvent (KeyCode.DownArrow, new KeyEvent (OnDownArrow), KEY_PRESSED);
		_input.RegisterKeyEvent (KeyCode.LeftArrow, new KeyEvent (OnLeftArrow), KEY_PRESSED);
		_input.RegisterKeyEvent (KeyCode.RightArrow, new KeyEvent (OnRightArrow), KEY_PRESSED);

		// 방향키가 눌렸을 때 플레이어의 상태를 PlayerState.ON_MOVE 로 전이.
		_input.RegisterKeyEvent (KeyCode.UpArrow, new KeyEvent (KeyTyped), KEY_TYPED);
		_input.RegisterKeyEvent (KeyCode.DownArrow, new KeyEvent (KeyTyped), KEY_TYPED);
		_input.RegisterKeyEvent (KeyCode.LeftArrow, new KeyEvent (KeyTyped), KEY_TYPED);
		_input.RegisterKeyEvent (KeyCode.RightArrow, new KeyEvent (KeyTyped), KEY_TYPED);
		
		// 방향키를 모두 뗐을 때 플레이어의 상태를 PlayerState.IDLE 로 전이.
		_input.RegisterKeyEvent (KeyCode.UpArrow, new KeyEvent (KeyReleased), KEY_RELEASED);
		_input.RegisterKeyEvent (KeyCode.DownArrow, new KeyEvent (KeyReleased), KEY_RELEASED);
		_input.RegisterKeyEvent (KeyCode.LeftArrow, new KeyEvent (KeyReleased), KEY_RELEASED);
		_input.RegisterKeyEvent (KeyCode.RightArrow, new KeyEvent (KeyReleased), KEY_RELEASED);

		// 방향키를 입력하면 플레이어가 바라보는 방향을 변경.
		_input.RegisterKeyEvent (KeyCode.LeftArrow, new KeyEvent (EnqueueLeft), KEY_TYPED);
		_input.RegisterKeyEvent (KeyCode.RightArrow, new KeyEvent (EnqueueRight), KEY_TYPED);
		_input.RegisterKeyEvent (KeyCode.UpArrow, new KeyEvent (EnqueueUp), KEY_TYPED);
		_input.RegisterKeyEvent (KeyCode.DownArrow, new KeyEvent (EnqueueDown), KEY_TYPED);
		
		_input.RegisterKeyEvent (KeyCode.LeftArrow, new KeyEvent (DequeueLeft), KEY_RELEASED);
		_input.RegisterKeyEvent (KeyCode.RightArrow, new KeyEvent (DequeueRight), KEY_RELEASED);
		_input.RegisterKeyEvent (KeyCode.UpArrow, new KeyEvent (DequeueUp), KEY_RELEASED);
		_input.RegisterKeyEvent (KeyCode.DownArrow, new KeyEvent (DequeueDown), KEY_RELEASED);
	}

	#region Key Toggle
	// 방향키가 눌렸다면 플레이어의 상태를 PlayerState.ON_MOVE로 전이.
	void KeyTyped () {
		pressedKeys += 1;
		PlayerState state = _stateManager.State;
		if (state == PlayerState.IDLE)
			Movable ();
	}
	// 방향키가 떼진다면 플레이어의 상태를 PlayerState.IDLE로 전이.
	void KeyReleased () {
		pressedKeys -= 1;
		PlayerState state = _stateManager.State;
		if (pressedKeys == 0 && state == PlayerState.ON_MOVE)
			Stop ();
	}
	#endregion

	#region Player Direction
	// keyList			입력한 방향키를 차례대로 저장
	// Enqueue(...)		입력한 방향키를 차례대로 저장
	// Dequeue(...)		방향키를 떼면 keyList에서 제거
	// ChangeDirection	가장 최근 입력한 방향키를 향하도록 방향을 바꾼다
	private List <PlayerDirection> keyList;
	private void Enqueue (PlayerDirection direction) {
		keyList.Add (direction);
		ChangeDirection ();
	}
	private void Dequeue (PlayerDirection direction) {
		keyList.Remove (direction);
		ChangeDirection ();
	}
	private void ChangeDirection () {
		if (keyList.Count != 0) {
			_stateManager.Direction = keyList [keyList.Count - 1];
		}
	}

	// Enqueue(...)의 래핑 함수
	private void EnqueueLeft() { Enqueue (PlayerDirection.LEFT); }
	private void EnqueueRight() { Enqueue (PlayerDirection.RIGHT); }
	private void EnqueueUp() { Enqueue (PlayerDirection.UP); }
	private void EnqueueDown() { Enqueue (PlayerDirection.DOWN); }

	// Dequeue(...)의 래핑 함수
	private void DequeueLeft () { Dequeue (PlayerDirection.LEFT); }
	private void DequeueRight () { Dequeue (PlayerDirection.RIGHT); }
	private void DequeueUp () { Dequeue (PlayerDirection.UP); }
	private void DequeueDown () { Dequeue (PlayerDirection.DOWN); }
	#endregion

	#region Key Events
	// 방향키를 입력하면 캐릭터를 이동시킨다.
	void OnUpArrow() {
		Move (Vector3.up * VERTICAL_SPEED);
	}
	void OnDownArrow() {
		Move (Vector3.down * VERTICAL_SPEED);
	}
	void OnLeftArrow() {
		Move (Vector3.left * HORIZONTAL_SPEED);
	}
	void OnRightArrow() {
		Move (Vector3.right * HORIZONTAL_SPEED);
	}
	#endregion

	#region Move Methods
	// 캐릭터를 이동.
	private void Move (Vector3 dir) {
		if (_stateManager.State != PlayerState.IDLE
			&& _stateManager.State != PlayerState.ON_MOVE)
			return;
		Vector3 nextPosition = transform.position + dir * Time.deltaTime;
		if (OutOfRange (nextPosition))
			return;
		transform.position = nextPosition;
	}
	// 캐릭터가 이동 범위 바깥으로 나갔는지 확인.
	private bool OutOfRange (Vector3 position) {
        return !pathGenerator.AvailableToMove(position);
	}
	// 플레이어의 상태를 전이.
	private void Movable () {
		_stateManager.ChangeState = PlayerState.ON_MOVE;
	}
	// 플레이어의 상태를 전이.
	private void Stop () {
		_stateManager.ChangeState = PlayerState.IDLE;
	}
	#endregion
}