using UnityEngine;
using System.Collections;

public enum PlayerAnimationType {
	IDLE,
	ON_MOVE,
	ON_CLIMB,
	ON_DROP,
	ON_CROW_LOST,
	ON_BEAR_BLACK_OUT
}

public class PlayerAnimationManager : MonoBehaviour {

	#region Singleton
	static public PlayerAnimationManager Instance {
		get {
			return _instance;
		}
	}
	static private PlayerAnimationManager _instance = null;
	#endregion

	public bool ON_ANIMATION;
	public PlayerAnimationType changeAnimation { set { nextAnimationType = value; } }
	private PlayerAnimationType animationType, nextAnimationType;
	private bool firstFrame;

	void Awake () {
		if (_instance == null)
			_instance = this;

		ON_ANIMATION = false;
		firstFrame = true;
		animationType = PlayerAnimationType.IDLE;
	}

	// Update is called once per frame
	void Update () {
		if (ON_ANIMATION == false)
			return;
		float dt = Time.deltaTime;
		switch (animationType) {
		case PlayerAnimationType.IDLE: ANIMATION_IDLE(dt); break;
		case PlayerAnimationType.ON_MOVE: ANIMATION_ON_MOVE(dt); break;
		case PlayerAnimationType.ON_CLIMB: ANIMATION_ON_CLIMB(dt); break;
		case PlayerAnimationType.ON_DROP: ANIMATION_ON_DROP(dt); break;
		case PlayerAnimationType.ON_CROW_LOST: ANIMATION_ON_CROW_LOST(dt); break;
		case PlayerAnimationType.ON_BEAR_BLACK_OUT: ANIMATION_ON_BEAR_BLACK_OUT(dt); break;
		default:
			break;
		}
	}

	void LateUpdate() {
		if (animationType != nextAnimationType) {
			animationType = nextAnimationType;
			firstFrame = true;
			return;
		}
		if (firstFrame)
			firstFrame = false;
	}

	#region Animation Methods
	private void ANIMATION_IDLE (float dt) {
		if (firstFrame) {
		}
	}
	private void ANIMATION_ON_MOVE (float dt) {
	}
	private void ANIMATION_ON_CLIMB (float dt) {
	}
	private void ANIMATION_ON_DROP (float dt) {
	}
	private void ANIMATION_ON_CROW_LOST (float dt) {
	}
	private void ANIMATION_ON_BEAR_BLACK_OUT (float dt){
	}
	#endregion
}
