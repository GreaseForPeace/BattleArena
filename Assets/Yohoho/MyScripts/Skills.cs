using UnityEngine;
using System.Collections;

public class Skills : MonoBehaviour
{

    public string Name;
    public int Damage;

	// Use this for initialization
	void Start (){
	}
	
	// Update is called once per frame
	void Update (){

    if (Input.GetKeyUp("1"))
	    {
	        FirstSkill(gameObject);
	    }
    if(Input.GetKeyUp("2"))
	    {
            SecondSkill(gameObject);
	    }
    if(Input.GetKeyUp("3"))
	    {
            ThirdSkill(gameObject);
	    }
    if(Input.GetKeyUp("4"))
	    {
            FourthSkill(gameObject);
	    }
    if(Input.GetKeyUp("5"))
	    {
            FifthSkill(gameObject);
	    }
    if(Input.GetKeyUp("6"))
	    {
            SixthSkill(gameObject);
	    }
	}

    static void FirstSkill( GameObject hero )
    {
        
    }

    static void SecondSkill( GameObject hero )
    {

    }

    static void ThirdSkill( GameObject hero )
    {

    }

    static void FourthSkill( GameObject hero )
    {

    }

    static void FifthSkill( GameObject hero )
    {

    }

    static void SixthSkill( GameObject hero )
    {

    }
}
