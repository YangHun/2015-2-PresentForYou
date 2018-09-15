using UnityEngine;
using System.Collections;

// PlayerStateManager의 FSM을 구성하는 상태들.
public enum PlayerState {
	IDLE, // 가만히 있을 때
	ON_MOVE, // 움직이는 도중
	ON_TREE, // 나무에 매달려 있는 중
	ON_CLIMB, // 나무를 타고 올라가는 중
	ON_DROP, // 나무에서 내려오는 (혹은 떨어지는) 중
	ON_CROW_LOST, // 까마귀와 부딪혔을 때
	ON_BEAR_BLACK_OUT // 곰과 부딪혔을 때
}

// 플레이어가 바라보는 방향
public enum PlayerDirection {
	LEFT, RIGHT, UP, DOWN
}

/// <summary>
/// PlayerStateManager는 플레이어의 상태를 관리합니다.
/// PlayerStateManager는 FSM으로 제작되었습니다.
/// 외부에서는 오로지 State를 통해 FSM의 현재 상태를 가져오고,
/// ChangeState를 통해 FSM의 다음 상태를 지정할 수 있습니다.
/// </summary>
public class PlayerStateManager : MonoBehaviour {
  
    #region TAG
    // 충돌한 물체를 태그를 이용해 판별하기 위한 문자열들.
    // CROW_TAG				까마귀의 태그.
    // BEAR_TAG				곰의 태그.
    // SNOW_CRYSTAL_TAG		얼음꽃의 태그.
    const string CROW_TAG = "Crow";
	const string BEAR_TAG = "Bear";
	const string SNOW_CRYSTAL_TAG = "Snow Crystal";
	#endregion

	#region Animation & Direction
    // PlayAnimation    state와 direction에 맞게 애니메이션 재생
    private void PlayAnimation(PlayerState state, PlayerDirection direction) {
        switch (state) {
            case PlayerState.IDLE:
            case PlayerState.ON_MOVE:
				if( _animator != null) _animator.Play(state.ToString() + "_" + direction.ToString());
                break;
        }
    }
    
    private void PlayAnimation(PlayerState state) {
        switch (state) {
            case PlayerState.ON_TREE:
            case PlayerState.ON_CLIMB:
            case PlayerState.ON_DROP:
            case PlayerState.ON_CROW_LOST:
            case PlayerState.ON_BEAR_BLACK_OUT:
                _animator.Play(state.ToString());
                break;
        }
    }

	// 플레이어가 바라보는 방향을 저장하고, 여기에 맞는 애니메이션을 재생하는 함수.
	// Direction	PlayerMove 스크립트에서 입력에 따라 플레이어의 방향을 바꿔준다.
	//				만약 대기 혹은 이동 중이라면 그에 맞는 애니메이션으로 바꿔준다.
	// _direction	플레이어가 바라보는 방향
	public PlayerDirection Direction {
		set {
            if (_direction != value) {
                _direction = value;
                PlayAnimation(state, _direction);
            }
        }
	}

	[SerializeField]
	private PlayerDirection _direction;
	#endregion

	#region STATE
	// State		외부에서 플레이어의 현재 상태를 읽기 위한 접근자.
	// ChangeState	외부에서 플레이어의 다음 상태를 지정하기 위한 수정자.
	[SerializeField]
	public PlayerState State { get { return state; } }
	public PlayerState ChangeState { set { nextState = value; } }

	// state		현재 플레이어의 상태.
	// nextState	다음 프레임에서의 상태.
	// firstFrame	현 상태에서 첫 프레임인지를 가리키는 플래그 비트.
	private PlayerState state, nextState;
	private bool firstFrame;
	#endregion

	#region Components of Player
	// _actionBehavior	같은 플레이어 게임 오브젝트에 포함된 PlayerAction 스크립트.
	// _inventory		같은 플레이어 게임 오브젝트에 포함된 PlayerInventory 스크립트.
	// _animator		같은 플레이어 게임 오브젝트에 포함된 Animator 컴포넌트.
	private PlayerAction _actionBehavior;
	private PlayerInventory _inventory;
	private Animator _animator;
	#endregion

	// 초기화.
	void Awake () {
		// 시작할 때 플레이어의 상태는 가만히 서있는 상태.
		// 별다른 조작이 없다면 다음 프레임에서도 여전히 가만히 서있는다.
		// 초기화한 직후를 첫 프레임으로 간주한다.
		// 시작할 때 찬 공기는 0%로 시작한다.
		// 보는 방향은 오른쪽으로 한다.
		state = PlayerState.IDLE;
		nextState = PlayerState.IDLE;
		firstFrame = true;
		coldAir = 0.0f;
		_direction = PlayerDirection.RIGHT;
       	// 동일 플레이어 게임 오브젝트의 스크립트들과 컴포넌트를 미리 탐색하고 저장.
		// 스크립트에 접근할 때마다 찾는 것은 비효율적.
		_actionBehavior = GetComponent<PlayerAction> ();
		_inventory = GetComponent<PlayerInventory> ();
		_animator = GetComponent<Animator> ();

        // 나무의 크기를 구해서 플레이어가 올라갈 높이를 구한다.
		float treeHeight = GameObject.FindGameObjectWithTag("Tree").GetComponent<PolygonCollider2D>().bounds.size.y;
		Debug.Log("tree height : " + treeHeight);
        for (int i = 1; i <= 4; i++) 
            TREE_HEIGHT[i] *= treeHeight;
	}

	// 매 프레임마다 호출된다.
	void Update () {
		// 매 프레임마다 찬 공기를 일정량 얻는다.
		UpdateColdAir ();

		// 플레이어의 현재 상태에 맞는 행동을 취한다.
		switch (state) {
		case PlayerState.IDLE: STATE_IDLE(); break;
		case PlayerState.ON_MOVE: STATE_ON_MOVE(); break;
		case PlayerState.ON_TREE: STATE_ON_TREE(); break;
		case PlayerState.ON_CLIMB: STATE_ON_CLIMB(); break;
		case PlayerState.ON_DROP: STATE_ON_DROP(); break;
		case PlayerState.ON_CROW_LOST: STATE_ON_CROW_LOST(); break;
		case PlayerState.ON_BEAR_BLACK_OUT: STATE_ON_BEAR_BLACK_OUT(); break;
		default:
			// 플레이어의 상태에 따른 행동이 구현 안 됐다.
			Debug.LogError ("Not Implemented.");
			break;
		}
	}

	// 모든 Update()가 호출되고 마지막으로 호출된다.
	void LateUpdate () {
		// 현재 상태와 다음 상태가 다르면 상태를 이전시킨다.
		if (state != nextState) {
			Debug.Log ("State changed to " + nextState);

			state = nextState;

			// 새로운 상태에서 첫 프레임으로 지정.
			firstFrame = true;
			return;
		}

		// 첫 프레임이 지났으므로 firstFrame 플래그 비트를 수정한다.
		if (firstFrame)
			firstFrame = false;
	}

	// 트리거를 작동시켰을 때 호출된다.
	void OnTriggerEnter2D (Collider2D collider) {
		// height			현재 플레이어가 나무를 얼마만큼 올랐는지.
		// snowCrystals		현재 플레이어가 보유한 얼음꽃의 개수.
		int height = _actionBehavior.Height;
		int snowCrystals = _inventory.NumberOfSnowCrystals;

        // 만약 까마귀와 충돌했을 경우.
        if (collider.CompareTag (CROW_TAG)) {
			// 높이가 3 미만일 때는 까마귀와 부딪히지 않은 것으로 판단한다.
			if (height < 3)
				return;
			// 얼음꽃을 가지고 있다면 일정 확률로 얼음꽃을 하나 잃는다.
			if (snowCrystals > 0 && Crow_lost_Invoked ())
				Crow_lost ();
			// 일정 확률로 플레이어가 나무에서 떨어진다.
			if (Crow_drop_Invoked ())
				Crow_drop ();

			// 만약 곰과 충돌했을 경우.
		} else if (collider.CompareTag (BEAR_TAG)) {
			// 높이가 3 이상일 경우 곰이 공격하지 못한 것으로 판단한다.
			if (height >= 3)
				return;
			// 곰과 충돌하면 무조건 기절한다.
			Bear_black_out ();

			// 만약 얼음꽃과 충돌했을 경우.
		} else if (collider.CompareTag (SNOW_CRYSTAL_TAG)) {
			// 해당 얼음꽃을 없앤다.
			// 그리고 보유한 얼음꽃의 개수를 하나 늘린다.
			DestroyObject (collider.gameObject);
			Snow_get();
		}
	}

	#region Cold Air
	// coldAir	플레이어가 소지한 찬 공기의 양.
	public float coldAir;

	// 매 프레임마다 찬 공기를 차게 한다.
    public void init_coldAir() {
        coldAir = 0;
    }
	private void UpdateColdAir () {
		// 찬 공기를 100% 가졌다면 더 이상 늘지 않는다.
		if (coldAir == 100.0f)
			return;
		
		// TODO: _stage를 외부에서 설정하도록 지정.

		// _stage	현재 스테이지의 숫자.
		// dt		지난 프레임의 Update() 호출로부터 이번 프레임의 Update() 호출까지 걸린 시간.
		int _stage = 1;
		float dt = Time.deltaTime;

		// 기획서에 등재된 공식에 따라 찬 공기를 얻는다.
		// 찬 공기가 100%를 넘을 수 없으므로 100%가 넘었다면 100%로 맞춘다.
		coldAir += Mathf.Pow (2.0f, _stage - 1) * dt;
		if (coldAir > 100.0f)
			coldAir = 100.0f;
	}
	#endregion

	#region Methods Invoked In Each States
	// 플레이어가 가만히 서있을 때.
	private void STATE_IDLE () {
		if (firstFrame) {
            // 대기 상태에서는 방향에 따른 애니메이션이 다르다.
            PlayAnimation(PlayerState.IDLE, _direction);
		}
	}

	// 플레이어가 움직일 때.
	private void STATE_ON_MOVE() {
		if (firstFrame) {
            // 이동 상태에서는 방향에 따른 애니메이션이 다르다.
            PlayAnimation(PlayerState.ON_MOVE, _direction);
		}
	}

	// 플레이어가 나무에 매달려 있을 때.
	private void STATE_ON_TREE() {
		if (firstFrame) {
            PlayAnimation(PlayerState.ON_TREE);
		}
	}

	// 플레이어가 나무를 올라갈 때.
	private void STATE_ON_CLIMB () {
		// 첫 프레임에서 애니메이션을 재생한다.
		// 그리고 나무에서의 높이를 하나 올리고,
		// 해당 높이로 플레이어 게임 오브젝트를 이동시킨다.
		if (firstFrame) {
            PlayAnimation(PlayerState.ON_CLIMB);

            if (_actionBehavior.Height == 0)
                onMove = true;
			_actionBehavior.Height += 1;
			SetMove ();
		}
		MoveTo (_actionBehavior.Height);
	}

	// 플레이어가 나무에서 떨어질 때.
	private void STATE_ON_DROP () {
		// 첫 프레임에서 애니메이션을 재생한다.
		// 그리고 나무에서의 높이를 0으로 설정하고,
		// 해당 높이로 플레이어 게임 오브젝트를 이동시킨다.
		if (firstFrame) {
            PlayAnimation(PlayerState.ON_DROP);

			SetMove ();
			_actionBehavior.Height = 0;
		}
		MoveTo (_actionBehavior.Height);
	}

	// 플레이어가 까마귀와 부딪힐 때.
	private void STATE_ON_CROW_LOST () {
		// 첫 프레임에서 애니메이션을 재생한다.
		// 나무를 오르는 도중 까마귀와 부딪히는 경우에도 나무를 올라가도록
		// MoveTo(...) 함수를 계속 호출한다.
		if (firstFrame) {
            PlayAnimation(PlayerState.ON_CROW_LOST);
		}
		MoveTo (_actionBehavior.Height);
	}

	// 플레이어가 곰과 부딪혔을 때.
	private void STATE_ON_BEAR_BLACK_OUT () {
		// 첫 프레임에서 애니메이션을 재생한다.
		if (firstFrame) {
            PlayAnimation(PlayerState.ON_BEAR_BLACK_OUT);
		}
		// TODO: 상위 관리자에게 플레이어가 기절했음을 알린다.
	}
    #endregion

    #region Climb On And Drop From Tree
    // onMove           플레이어가 나무를 향해 다가가는 중인지를 나타내는 플래그 비트.
    // onClimb			플레이어가 나무를 올라가거나, 나무에서 떨어지는 중인지를 나타내는 플래그 비트.
    // startPosition	플레이어가 이동하는 시작 위치
    // endPosition		플레이어가 이동할 도착 위치
    // moveSpeed		플레이어의 이동 속도
    // moveTime			플레이어가 이동한 시간.
    // moveDistance		플레이어가 이동할 거리.
    // tree             플레이어가 올라타는 나무 오브젝트
    // treeOrder        플레이어가 올라타는 나무 오브젝트의 TreeRendererOrder 컴포넌트
    private bool onMove = false;
    private bool onClimb = false;
	private Vector3 startPosition, endPosition;
	const float moveSpeed = 8.0f;
	private float moveTime;
	private float moveDistance;
    private GameObject tree;
    private TreeRendererOrder treeOrder;

	// TREE_HEIGHT [i]	나무 하단의 트리거로부터 높이 i까지의 거리.
	private float[] TREE_HEIGHT = {0.0f, 0.1f, 0.3f, 0.6f, 1.0f};

	// 플레이어가 이동할 경로를 지정.
	private void SetMove () {
        // 아직 나무 위가 아니면 나무로 이동
        if (onMove) {
            // 올라가려는 나무를 구함.
            tree = _actionBehavior.Tree;
            treeOrder = tree.GetComponent<TreeRendererOrder>();
            // startPosition을 현재 위치로 설정.
            // endPosition을 나무의 바닥 위치로 설정.
            // movdDistance를 시작 위치와 도착 위치 사이의 거리로 설정.
            // 아직 이동하지 않았으므로 이동 시간은 0.
            startPosition = transform.position;
            endPosition = new Vector3(startPosition.x, tree.transform.position.y, 0);
            moveDistance = Vector3.Distance(startPosition, endPosition);
            moveTime = 0.0f;
        }
        // 나무에 올라설 준비가 되면 위로 이동
        else {
            // startPosition을 현재 위치로 설정.
            // endPosition은 현재 상태를 고려해서 설정.
            // 		if (나무를 오르는 상태)
            //			도착 위치 = 현재 위치 + {높이 i와 높이 (i-1) 사이의 거리}
            //		else (나무를 내려가는 상태)
            //			도착 위치 = 현재 위치 - (높이 i부터 나무 하단 트리거까지의 거리)
            // moveDistance를 시작 위치와 도착 위치 사이의 거리로 설정.
            // 아직 이동하지 않았으므로 이동 시간은 0.
            // 플레이어가 이동하는 동작이 실행됐으므로 onMove 플래그 비트는 참으로 설정.

            int height = _actionBehavior.Height;
            startPosition = transform.position;
            if (state == PlayerState.ON_CLIMB)
                endPosition = startPosition + Vector3.up * (TREE_HEIGHT[height] - TREE_HEIGHT[height - 1]);
            else
                endPosition = new Vector2(startPosition.x, tree.transform.position.y - 0.1f);
            moveDistance = Vector3.Distance(startPosition, endPosition);
            moveTime = 0.0f;
            onClimb = true;
        }
    }

	// SetMove()에서 설정한 경로를 따라가는 함수.
	private void MoveTo (int height) {
        // 플레이어가 나무를 향해 다가가는 중이면 실행한다.
        if (onMove) {
            // 이동 시간을 추가하고, 
            // 시작 지점을 0, 도착 지점을 1로 봤을 때 현재 있을 위치를 보간법을 이용해 산출하고 적용.
            moveTime += Time.deltaTime;
            float distMoved = moveTime * moveSpeed;
            float fracMoved = distMoved / moveDistance;

            transform.position = Vector3.Lerp(startPosition, endPosition, fracMoved);

            // 보간법으로는 오차가 발생하므로 일정 수준의 오차는 무시하고 도착한 것으로 간주한다.
            if (Arrive()) {
                // 나무가 흐려지지 않게 잠근다.
                treeOrder.Lock = true;
                treeOrder.ClimbingObject = this.gameObject;
                onMove = false;
                SetMove();
            }
        }
        // 플레이어가 나무를 올라가는 중이면 실행한다.
        else if (onClimb) {
            // 이동 시간을 추가하고,
            // 시작 지점을 0, 도착 지점을 1로 봤을 때 현재 있을 위치를 보간법을 이용해 산출하고 적용.
            // 사용한 보간법은 선형보간법 Vector3.Lerp (...).
            moveTime += Time.deltaTime;
            float distMoved = moveTime * moveSpeed;
            float fracMoved = distMoved / moveDistance;

            transform.position = Vector3.Lerp(startPosition, endPosition, fracMoved);
            //		transform.position = Vector3.Lerp (startPosition, endPosition, moveTime * 3);

            // 보간법으로는 오차가 발생하므로 일정 수준의 오차는 무시하고 도착한 것으로 간주한다.
            if (Arrive()) {
                onClimb = false;
                if (state == PlayerState.ON_CLIMB || state == PlayerState.ON_CROW_LOST)
                    ChangeState = PlayerState.ON_TREE;
                else {
                    // 나무의 잠금을 해제한다.
                    treeOrder.Lock = false;
                    ChangeState = PlayerState.IDLE;
                }
            }
        }
    }

	// 일정 수준의 오차를 무시하고 도착 위치에 도달했는지 판단.
	private bool Arrive () {
		return Vector3.Distance (transform.position, endPosition) <= moveDistance * Time.deltaTime;
	}
	#endregion

	#region Interaction With Crow
	// 일정 확률에 따라 얼음꽃을 잃어버릴지 판단.
	private bool Crow_lost_Invoked () {
		// TODO: 상위 관리자에서 스테이지의 번호 n을 받아온다.
		// n			스테이지 번호.
		// probability	0~100 사이의 무작위 숫자.
		int n = 1;
		int probability = Random.Range (0, 100);
		return (probability <= 25 + (5 * n));
	}
	// 까마귀와 부딪혀 얼음꽃을 잃어버릴 때.
	private void Crow_lost () {
		_inventory.LoseSnowCrystal (1);
		ChangeState = PlayerState.ON_CROW_LOST;
	}

	// 일정 확률에 따라 나무에서 떨어질지 판단.
	private bool Crow_drop_Invoked () {
		// probability	0~100 사이의 무작위 숫자.
		int probability = Random.Range (0, 100);
		return (probability <= 5);
	}
	// 까마귀와 부딪혀 나무에서 떨어질 때.
	private void Crow_drop () {
		ChangeState = PlayerState.ON_DROP;
	}
	#endregion

	#region Interaction With Bear
	// 곰에 맞아 정신을 잃을 때.
	private void Bear_black_out() {
		// TODO: 시스템 시간 경과.

		// 모든 찬 공기를 잃는다.
		coldAir = 0.0f;
		ChangeState = PlayerState.ON_BEAR_BLACK_OUT;
	}
	#endregion

	#region Interaction With Snow Crystal
	// 얼음꽃과 부딪혔을 때
	private void Snow_get () {
		_inventory.GetSnowCrystal (1);
		Debug.Log ("snow crystal (" + _inventory.NumberOfSnowCrystals + ")");
	}
	#endregion
}
