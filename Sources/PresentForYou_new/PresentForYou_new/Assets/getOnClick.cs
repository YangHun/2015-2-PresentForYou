using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class getOnClick : MonoBehaviour {

    void getonClick(Button B) {
        var A=GameObject.Find("UI Manager").GetComponent<UIManager>();
        A.Stage(B);
    }
}
