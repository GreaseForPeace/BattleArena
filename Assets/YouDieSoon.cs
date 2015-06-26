using UnityEngine;
using System.Collections;

public class YouDieSoon : MonoBehaviour {

    public float Timer = 2;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Timer -= Time.deltaTime;

        if (Timer <= 0)
        {
            Destroy(gameObject);
        }
	}
}
