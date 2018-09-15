using UnityEngine;
using System.Collections.Generic;

/// Tree의 렌더링 오더를 정하는 스크립트.
/// Lock    트리 변화를 막는 플래그    
public class TreeRendererOrder : MonoBehaviour {

    // Alpha            알파값의 프로퍼티, 렌더러의 알파값도 수정
    // alpha            알파값
    private float Alpha {
        get {
            return alpha;
        }
        set {
            if (alpha != value) {
                alpha = value;
                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }
        }
    }
    private float alpha = 1.0f;

    // transparent      투명화 여부 플래그
    // Lock             나무의 변화를 막는 플래그
    // ClimbingObject   현재 올라가 있는 물체 프로퍼티
    // climbingObject   현재 올라가 있는 물체
    private bool transparent = false;
    public bool Lock { get; set; }
    public GameObject ClimbingObject { set { climbingObject = value; } }
    private GameObject climbingObject;

    // colliders        현재 겹친 게임 오브젝트들
    private HashSet<GameObject> colliders = new HashSet<GameObject>();

    // spriteRenderer   SpriteRenderer 컴포넌트
    private SpriteRenderer spriteRenderer;

    
    void Start () {
        Lock = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update () {
        transparent = false;

        if (colliders.Count > 0)
            foreach (GameObject obj in colliders)
                Reorder(obj);

        OrderPlayer();

        if (Lock) 
            transparent = false;

        UpdateAlpha();
    }

    void OnTriggerEnter2D (Collider2D collider) {
        if (collider.tag == "Player" || collider.tag == "Bear")
            colliders.Add(collider.gameObject);
    }

    void OnTriggerExit2D (Collider2D collider) {
        if (colliders.Contains(collider.gameObject))
            colliders.Remove(collider.gameObject);
    }


    // 위치에 따라 렌더링 순서를 변화
    void Reorder(GameObject obj) {
        // 나무보다 앞에 있을 때
        if (obj.transform.position.y <= transform.position.y)
            obj.GetComponent<SpriteRenderer>().sortingOrder = spriteRenderer.sortingOrder + 1;
        // 나무보다 뒤에 있을 때
        else {
            obj.GetComponent<SpriteRenderer>().sortingOrder = spriteRenderer.sortingOrder - 1;
            transparent = true;
        }
    }

    // 현재 올라가있는 플레이어의 렌더링 순서를 변화.
    void OrderPlayer() {
        if (Lock)
            climbingObject.GetComponent<SpriteRenderer>().sortingOrder = spriteRenderer.sortingOrder + 1;
        else
            if (climbingObject != null)
                climbingObject = null;
    }

    // alpha 값을 상황에 맞게 변화
    void UpdateAlpha() {
        if (transparent) {
            if (Alpha > 0.5f) {
                Alpha -= Time.deltaTime * 3f;
                if (Alpha < 0.5f)
                    Alpha = 0.5f;
            }
        }
        else {
            if (Alpha < 1.0f) {
                Alpha += Time.deltaTime * 3f;
                if (Alpha > 1.0f)
                    Alpha = 1.0f;
            }
        }
    }
}
