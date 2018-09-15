using UnityEngine;
using System.Collections.Generic;

public class PlayerAction : MonoBehaviour {

	// TREE_TAG			나무의 태그.
	// HEIGHT_OF_TREE	나무의 높이.
	const string TREE_TAG = "Tree";
	const int HEIGHT_OF_TREE = 4;

	// ctrlPressed	Ctrl 키가 눌렸는지 나타내는 플래그 비트.
	// nearTree		근처에 있는 나무의 개수.
    // TreePosition 근처에 있는 가장 마지막 나무의 위치.
    // treePos      근처에 있는 나무들의 위치.
	// Height		외부에서 height에 접근하기 위한 접근자.
	// height		현재 플레이어의 높이.
	private bool ctrlPressed = false;
	private int nearTree = 0;
    public GameObject Tree { get { return nearTree > 0 ? trees[trees.Count - 1] : null; } }
    private List<GameObject> trees = new List<GameObject>();
	public int Height { get { return height; } set { height = value; } }
	private int height = 0;

	private PlayerStateManager _stateManager;

	// 초기화.
	void Start () {
		const int KEY_TYPED = (int)KeyEventType.KeyTyped;
		const int KEY_RELEASED = (int)KeyEventType.KeyReleased;

		PlayerInputManager _input = GetComponent <PlayerInputManager> ();
		_stateManager = GetComponent <PlayerStateManager> ();
		
		_input.RegisterKeyEvent (KeyCode.LeftControl, new KeyEvent (CtrlTyped), KEY_TYPED);
		_input.RegisterKeyEvent (KeyCode.LeftControl, new KeyEvent (CtrlReleased), KEY_RELEASED);
		
		_input.RegisterKeyEvent (KeyCode.Space, new KeyEvent (Tree_climb), KEY_TYPED);
		_input.RegisterKeyEvent (KeyCode.Space, new KeyEvent (Tree_drop), KEY_TYPED);
		_input.RegisterKeyEvent (KeyCode.X, new KeyEvent (Tree_drop), KEY_TYPED);
	}

	// 근처에 나무가 나타날 때.
	void OnTriggerEnter2D(Collider2D collider) {
		if (collider.CompareTag (TREE_TAG)) { 
            if (!trees.Contains(collider.gameObject)) {
                trees.Add(collider.gameObject);
                nearTree++;
                Debug.Log("Near Tree ++ | " + nearTree);
            }
        }
    }

    // 근처에 나무가 없어질 때.
    void OnTriggerExit2D(Collider2D collider) {
		if (collider.CompareTag (TREE_TAG)) { 
            if (trees.Contains(collider.gameObject)) {
                trees.Remove(collider.gameObject);
                nearTree--;
                Debug.Log("Near Tree -- | " + nearTree);
            }
        }
    }

	#region Ctrl Toggle
	// Ctrl이 눌릴 때와 떼질 때.
	void CtrlTyped () { ctrlPressed = true; }
	void CtrlReleased () { ctrlPressed = false; }
	#endregion

	#region Interaction with Tree
	// 나무를 올라갈 때. 상태를 전이시켜 놓으면 PlayerStateManager에서 나무를 올라가는 동작을 처리.
	void Tree_climb () {
		if (ctrlPressed || nearTree + height <= 0 || height == HEIGHT_OF_TREE)
			return;
		PlayerState state = _stateManager.State;
		if (state == PlayerState.IDLE
			|| state == PlayerState.ON_MOVE
			|| state == PlayerState.ON_TREE) {
			_stateManager.ChangeState = PlayerState.ON_CLIMB;
		}
	}
	// 나무에서 내려올 때. 상태를 전이시켜 놓으면 PlayerStateManager에서 나무를 내려가는 동작을 처리.
	void Tree_drop () {
		if ((Input.GetKeyDown(KeyCode.X) == false && ctrlPressed == false)
		    || _stateManager.State != PlayerState.ON_TREE)
			return;
		_stateManager.ChangeState = PlayerState.ON_DROP;
	}
	#endregion
}
