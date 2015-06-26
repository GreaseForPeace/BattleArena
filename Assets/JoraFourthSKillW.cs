using UnityEngine;
using System.Collections;
using Assets.Yohoho.MyScripts;

public class JoraFourthSKillW : BasicSkill
{

    public AnimationClip BoomMan;
    public GameObject BOOM;
    public float Timer;
    private float SaveTimer;
    private bool _isActive = false;

	// Use this for initialization
	void Start () {
        SaveTimer = Timer;
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown("4") && !_isActive)
        {
            _isActive = true;
            animation.Play(BoomMan.name);
        }
        if (_isActive) {
            
            Timer -= Time.deltaTime;
            if (Timer <= 0)
            {
                Instantiate(BOOM, transform.position, transform.rotation);
                animation.Stop();
                _isActive = false;
                Timer = SaveTimer;
            }
        
        }
	}
}
