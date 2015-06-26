using UnityEngine;
using System.Collections;
using Assets.Yohoho.MyScripts;

public class JoraFifthSkill : BasicSkill
{
    public float Timer;
    private float SaveTimer;
    private float rootTimer;
    public float BulletForce;
    private bool _isActive = false;
    public AnimationClip StrongArm;
    public GameObject Trigger;


    private Ray ray;
    private RaycastHit hit;
	// Use this for initialization
	void Start () {
        SaveTimer = Timer;
	}
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown("5") && !_isActive)
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);        //Цель обнаружена
            if (Physics.Raycast(ray, out hit, 10000.0f))                    //ОГОНЬ!
            {
              //  animation.Play(StrongArm.name);
                Quaternion qua;
                var bullet = (GameObject)Instantiate(Trigger, gameObject.transform.position, gameObject.transform.rotation + qua); // Само создание Снаряда
                 bullet.rigidbody.AddForce(transform.forward * BulletForce);
                _isActive = true;
            }
        }

        if (_isActive) {

     //       bullet.position = Vector3.Slerp(bullet.position, hit.point, 1f - rootTimer);
     //       rootTimer -= Time.deltaTime / Timer;
            
       }
        

        if (rootTimer <= 0 && _isActive)
        {
       //     Destroy(bullet.gameObject);
            _isActive = false;
        }
	}
}
