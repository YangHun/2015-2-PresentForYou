using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// BearStateManager의 FSM을 구성하는 상태들
public enum BearState {
    IDLE,   // 제자리 정지
    WANDER, // 두리번 두리번
    PATROL, // HFSM 요소. IDLE과 WANDER을 관리
    CHASE   // 플레이어 쫓아갈 때
}

// 곰이 바라보는 방향
public enum BearDirection {
    LEFT, RIGHT, UP, DOWN
}


/// <summary>
/// BearStateManager는 곰의 상태를 관리합니다.
/// HFSM으로 작성되었으며, 외부에서 State를 통해 FSM의 현재 상태를 가져오고
/// ChangeState를 통해 FSM의 다음 상태를 지정할 수 있습니다.
/// </summary>
public class BearStateManager : MonoBehaviour {
    
    #region Animation Tag
    // PlayBearAnimation    state와 direction에 맞게 애니메이션 재생   
    private void PlayBearAnimation() {
        if (state == BearState.PATROL) return;
        _animator.Play(state.ToString() + "_" + _direction.ToString());
    }
    #endregion

    #region Direction 
    // Direction        방향 수정자
    public BearDirection Direction {
        set {
            if (_direction != value) {
                _direction = value;
                PlayBearAnimation();
            }
        }
        get {
            return _direction;
        }
    }
    private BearDirection _direction;

    // ToBearDirection  Vector2 형태의 방향을 BearDirection으로 변환한다.
    private BearDirection ToBearDirection (Vector2 dir) {
        Vector2 direction = dir.normalized;
        float hflag = Vector2.Dot(Vector2.right, direction);
        float vflag = Vector2.Dot(Vector2.up   , direction);
        if (Mathf.Abs(vflag) > Mathf.Abs(hflag))
            return vflag > 0 ? BearDirection.UP : BearDirection.DOWN;
        else
            return hflag > 0 ? BearDirection.RIGHT : BearDirection.LEFT;
    }

    // RandomDirection  랜덤하게 BearDirection을 구한다.
    private BearDirection RandomDirection() {
        switch (Random.Range(0, 4)) {
            case 0: return BearDirection.UP;
            case 1: return BearDirection.DOWN;
            case 2: return BearDirection.LEFT;
            default: return BearDirection.RIGHT;
        }
    }

    // OnSight          곰의 시야에 플레이어가 있는지 여부를 확인한다.
    private bool IsPlayerOnSight() {
        // 플레이어가 나무위에 올라가있지 않은지 확인
        if (playerAction.Height < 2) {
            // 플레이어가 시야 거리 안에 있는지 확인
            Vector2 delta = player.transform.position - transform.position;
            if (delta.magnitude < sightDistance) {
                // 플레이어가 시야 각도 안에 있는지 확인
                Vector2 v;
                switch (_direction) {
                    case BearDirection.UP: v = Vector2.up; break;
                    case BearDirection.DOWN: v = Vector2.down; break;
                    case BearDirection.LEFT: v = Vector2.left; break;
                    case BearDirection.RIGHT: v = Vector2.right; break;
                    default: v = Vector2.zero; break;
                }
                if (Vector2.Dot(delta.normalized, v) > sightAngleCos) {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region State
    // State        상태 접근자
    // ChangeState  다음 상태 수정자
    public BearState State {
        set {
            if (state != value) {
                state = value;
                PlayBearAnimation();
            }
        }
        get {
            return state;
        }
    }
    public BearState ChangeState { set { nextState = value; } }

    // state        현재 상태
    // nextState    다음 프레임의 상태
    // firstFrame   첫 프레임인지 나타내는 flag
    [SerializeField]
	private BearState state, nextState;
    private bool firstFrame;
    #endregion

    #region Components 
    // _animator        gameObject에 포함된 Animator 컴포넌트
    // _rigidbody       gameObject에 포함된 Rigidbody2D 컴포넌트
    private Animator _animator;
    private Rigidbody2D _rigidbody;

    // sightDistance    곰의 시야 거리
    // sightAngleCos    곰의 시야 각도의 코사인 값
    // maxDestination   곰이 이동할 수 있는 최대 거리
    const float sightDistance  = 8f;
    const float sightAngleCos  = 0.3f;
    const float maxDestination = 10f;

    // activeFigure     곰의 활발함 정도 (IDLE과 WANDER의 비율)
    const float activeFigure = 0.4f;

    // originalPosition 곰이 처음 출발한 위치
    private Vector2 originalPosition;

    // pathGenerator    map에 포함된 PathGenerator 컴포넌트
    private PathGenerator pathGenerator;

    // player           Scene에 있는 Player 
    // playerAction     Player의 PlayerAction 컴포넌트
    private GameObject player;
    private PlayerAction playerAction;
    #endregion

    #region Move Method
    // Move         목적지로 이동
    // CancelMove   이동을 취소
    // Moving       이동중인지 나타내는 불리안
    void Move (Vector3 destination, float speed) {
        CancelMove();
        _followPath = FollowPath(pathGenerator.GetPath(transform.position, destination), speed);
        StartCoroutine(_followPath);
    }

    void CancelMove () {
        if (_followPath != null)
            StopCoroutine(_followPath);
        _isMoving = false;
    }

    bool Moving { get { return _isMoving; } }

    // _followPath      현재 경로를 이동하는 코루틴
    private IEnumerator _followPath;

    // _triggerSize     목적지에 도달한 걸 인식하는 범위
    // _lerpMagnitude   방향 전환의 부드러운 정도
    const float _triggerSize = 0.1f;
    const float _lerpMagnitude = 0.03f;

    // FollowPath       리스트로 주어진 경로를 따라 이동하는 코루틴
    private IEnumerator FollowPath (List<Vector2> path, float speed) {
        _isMoving = true;

        if (path != null) {
            foreach (Vector2 i in path) {
                // 각 체크포인트에 도달할 때까지 이동
                Vector2 v;
                while ((v = i - (Vector2)transform.position).magnitude > _triggerSize) {
                    v.Normalize();
                    v *= speed;
                    _rigidbody.velocity = (_rigidbody.velocity.magnitude < speed / 1.2f)
                            ? Vector2.Lerp(_rigidbody.velocity, v, 0.3f)
                            : Vector2.Lerp(_rigidbody.velocity, v, _lerpMagnitude * speed);
                    Direction = ToBearDirection(v);
                    yield return null;
                }
            }

            while (_rigidbody.velocity.magnitude > 0.1f) {
                yield return null;
                _rigidbody.velocity *= (_rigidbody.velocity.magnitude - 0.02f) / speed;
            }
        }
        else
            Debug.Log("Path is not exist");

        _rigidbody.velocity = Vector2.zero;
        _isMoving = false;
    }

    // MoveOnError      예외 발생시 곰을 맵 위로 돌아오게 합니다
    private IEnumerator MoveOnError () {
        _isMoving = true;
        _rigidbody.velocity = (originalPosition - (Vector2)transform.position).normalized * 4f;
        while (!pathGenerator.AvailableToMove(transform.position))
            yield return null;
        _rigidbody.velocity = Vector2.zero;
        _isMoving = false;
    }

    // _isMoving        현재 움직이고 있는지 나타내는 불리안 플래그
    bool _isMoving = false;
    #endregion


    // 초기화
    void Awake () {
        state = nextState = BearState.PATROL;
        firstFrame = true;
        _direction = RandomDirection();

        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody2D>();

        pathGenerator = GameObject.FindGameObjectWithTag("Map").GetComponent<PathGenerator>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerAction = player.GetComponent<PlayerAction>();

        originalPosition = transform.position;
    }

    
    void Update () {
	    switch (state) {
            case BearState.IDLE:
            case BearState.WANDER:
            case BearState.PATROL:
                State_PATROL();
                break;
            case BearState.CHASE:
                State_CHASE();
                break;
            // 미구현 상황
            default:
                Debug.LogError("Not implemented BearState");
                break;
        }

        // BUG: 게임 도중에 Bear가 맵 바깥으로 잠깐 나가는 사이에 순간이동해버리는 경우가 있습니다.
        // 임시방편으로 순간이동해버리면 원위치로 돌아오는거로...
        if (!pathGenerator.AvailableToMove(transform.position)) {
            State = nextState = BearState.WANDER;
            CancelMove();
            StartCoroutine(_followPath = MoveOnError());
        }
    }


    void LateUpdate () {
        // 현재 상태와 다음 상태가 다르면 상태를 이전시킨다.
        if (state != nextState) {
            Debug.Log("Bear state changed to " + nextState);
            State = nextState;
            firstFrame = true;
        }
        else {
            // 첫 프레임이 지났으므로 firstFrame flag을 수정
            if (firstFrame)
                firstFrame = false;
        }
    }
    
    #region Methods Invoked In Each States
    float _timer;

    // 곰의 Patrol 상태. IDLE과 WANDER을 관리
    private void State_PATROL() {
        if (firstFrame) {
            // IDLE과 WANDER 둘 중 랜덤으로 선택해 적용
            State = nextState = Random.value > activeFigure ? BearState.IDLE : BearState.WANDER;
            Debug.Log("Bear state changed to " + nextState);
        }
        switch (state) {
            case BearState.IDLE:   State_IDLE  (); break;
            case BearState.WANDER: State_WANDER(); break;
        }

        // 플레이어를 감지하면 상태를 변환
        if (IsPlayerOnSight())
            ChangeState = BearState.CHASE;
    }
    
    // 곰이 가만히 있을 때
    private void State_IDLE() {
        if (firstFrame) {
            _timer = Time.time;
            CancelMove();
            _rigidbody.velocity = Vector2.zero;
            Direction = RandomDirection();
        }
        // 2초가 지나면 상태를 변환
        if (Time.time - _timer >= 2f) {
            ChangeState = BearState.PATROL;
        }
    }

    // 곰이 주변을 탐색할 때
    private void State_WANDER() {
        if (firstFrame) {            
            // Move to random position
            Move(pathGenerator.RandomPosition(transform.position, 4), 2f);
        }
        // 이동이 끝나면 상태를 변환
        if (!Moving) {
            ChangeState = BearState.PATROL;
        }
    }

    // 곰이 플레이어를 쫒아갈 때
    private void State_CHASE() {
        if (firstFrame) {
            _timer = Time.time;
            Move(player.transform.position, 4f);
        }
        // 일정 시간마다 경로 갱신
        if (Time.time - _timer > 0.5f) {
            Move(player.transform.position, 4f);
            _timer = Time.time;
        }
        // 플레이어가 시야에서 벗어나면 상태를 변환
        if (!IsPlayerOnSight())
            ChangeState = BearState.PATROL;
    }
    #endregion
}
