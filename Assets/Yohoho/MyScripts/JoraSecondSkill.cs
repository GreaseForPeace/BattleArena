using UnityEngine;
using System.Collections;
using Assets.Yohoho.MyScripts;

public class JoraSecondSkill : BasicSkill
{

    public GameObject Trigger;
    public float Timer;
    private float SaveTimer;
    private bool _boolka = false;
    public AnimationClip Skill;
    public GameObject Effect;

	// Use this for initialization
	void Start ()
	{
        SaveTimer = Timer;
	}
	
	// Update is called once per frame
	void Update () {

	    if (Input.GetKeyDown("2"))
	    {
            animation.Play(Skill.name);
            Instantiate(Trigger, gameObject.transform.position, gameObject.transform.rotation); // Само создание сферы
            Instantiate(Effect, gameObject.transform.position, gameObject.transform.rotation);
	      //  gameObject.GetComponent<MovePlayer>().enabled = false;
	        _boolka = true;
	    }
	    if (_boolka)
	    {
	        Timer -= Time.deltaTime;
	        if (Timer <= 0)
	        {
                Destroy(GameObject.Find("Trigger(Clone)"));
                Destroy(GameObject.Find("ShockWave(Clone)"));
             //   gameObject.GetComponent<MovePlayer>().enabled = true;
                animation.Stop();
                if (!gameObject.GetComponent<MovePlayer>().enabled)
                {
                    gameObject.GetComponent<MovePlayer>().enabled = true;
                }
	            _boolka = false;
                Timer = SaveTimer;
	        }
	    }
	}
}
