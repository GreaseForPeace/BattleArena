using UnityEngine;
using System.Collections;
using Assets.Yohoho.MyScripts;

public class JoraSecondSkill : BasicSkill
{

    public GameObject Trigger;
    public float Timer;
    private bool _boolka = false;

	// Use this for initialization
	void Start ()
	{
        
	}
	
	// Update is called once per frame
	void Update () {

	    if (Input.GetKeyDown(KeyCode.A))
	    {
            Instantiate(Trigger, gameObject.transform.position, gameObject.transform.rotation); // Само создание сферы
	      //  gameObject.GetComponent<MovePlayer>().enabled = false;
	        _boolka = true;
	    }
	    if (_boolka)
	    {
	        Timer -= Time.deltaTime;
	        if (Timer <= 0)
	        {
                Destroy(GameObject.Find("Trigger(Clone)"));
             //   gameObject.GetComponent<MovePlayer>().enabled = true;
	            _boolka = false;
	        }
	    }
	}
}
