using UnityEngine;
using System.Collections;

public class TriggFift : MonoBehaviour {

   
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}
    void OnTriggerStay(Collider Si) {

        if (Si.gameObject.GetComponent<PlayerClass>()) {

            Debug.Log(Si.name + " получает уон");
        }
    
    }
}
