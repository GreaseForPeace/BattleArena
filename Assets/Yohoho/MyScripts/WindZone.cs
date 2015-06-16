using UnityEngine;
using System.Collections;

public class WindZone : MonoBehaviour {

	public Transform Player;
	public Vector3 Vector;

	void OnTriggerEnter(Collider c){

		if(c.gameObject.name == "NEWpers"){
			Player = c.transform;
		}
	}

	void OnTriggerExit(Collider col){

		if(col.gameObject.name == "NEWpers"){
			Debug.Log("MOZHEM");
			Player = null;
		}
	}

	void Update(){
		if(Player!=null){
			Debug.Log("OH YEA");
			CharacterController controller = Player.GetComponent<CharacterController>();
			Vector = new Vector3 (Player.position.x, Player.position.y + 5, Player.position.z);
			Vector = Player.TransformDirection (Vector);
			Vector *= 0.5f;
			controller.Move (Vector * Time.deltaTime);
		}
		else{
			Debug.Log("NE NADO");
		}
	}

}
