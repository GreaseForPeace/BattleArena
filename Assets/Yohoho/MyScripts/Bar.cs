using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Bar : MonoBehaviour {
  
    public Slider HBar2;


    void Start()
    {
        var hero = GetComponent<PlayerClass>();
        HBar2.minValue = 0;
        HBar2.maxValue = hero.MaxHp;
    }
	
	// Update is called once per frame
	void Update () {
        var hero = GetComponent<PlayerClass>();
        HBar2.value = hero.CurrHp;
	}
}
