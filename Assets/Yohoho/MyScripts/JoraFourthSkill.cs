using UnityEngine;
using System.Collections;
using Assets.Yohoho.MyScripts;

public class JoraFourthSkill : BasicSkill
{
    private bool _isActive;
    public float Timer = 3;
    private Quaternion fromRotate;
    private Quaternion toRotate;
    MovePlayer moveP;
    private float rotTimer = -1;
	// Use this for initialization
	void Start ()
	{
	    _isActive = false;
        fromRotate = transform.localRotation;
        toRotate = Quaternion.Euler(transform.rotation.x - 90, transform.rotation.y, transform.rotation.z);
        moveP = gameObject.GetComponent<MovePlayer>();
	    rotTimer = 1;
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetKeyDown("4") && _isActive == false) //Если нажато колесико мыши  и персонаж не в прыжке
	    {
	        Debug.Log("Ты нажал 4");
	        _isActive = true;
	        if (moveP.enabled)
	        {
	            moveP.enabled = false;
	            Debug.Log("Жорка обездвижен");
	        }
	    }

        if (rotTimer >= 0 && _isActive)
        {
            transform.rotation = Quaternion.Lerp(fromRotate, toRotate, 1f - rotTimer);
            rotTimer -= Time.deltaTime / Timer; //< -------- количество секунд, за которое надо повернуть
            Debug.Log(rotTimer);
            Debug.Log("ПЩЩЩЩ");
        }

        if (rotTimer <= 0 && _isActive)
        {
            Debug.Log("Это должно выполнится 1 раз");
            _isActive = false;
            moveP.enabled = true;
            rotTimer = 1;
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        
        
	}
}
