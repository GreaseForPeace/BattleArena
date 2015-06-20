using UnityEngine;
using System.Collections;
using Assets.Yohoho.MyScripts;

public class JoraThirdSkill : BasicSkill
{

    public float Duration;
    public int Value;
    private int _valueInPerc;
    private PlayerClass _hero;
    private bool IsActive = false;

	// Use this for initialization
	void Start ()
	{
	    _hero = gameObject.GetComponent<PlayerClass>();
	}
	
	// Update is called once per frame
	void Update () {

	    if (Input.GetKeyDown("3") && !IsActive)
	    {
	        Debug.Log("Защита Жорика увеличена с " + _hero.Armor);
	        _valueInPerc = (Value * _hero.Armor) / 100;
	        _hero.Armor += _valueInPerc;
	        Debug.Log("до " + _hero.Armor);
	        IsActive = true;
            
	    }

	    if (IsActive)
	    {
	        Duration -= Time.deltaTime;
	        if (Duration <= 0)
	        {
	            _hero.Armor -= _valueInPerc;
                Debug.Log("Защита Жорика возвращена до " + _hero.Armor);
	            IsActive = false;   
	        }
	    }
        
	}
}
