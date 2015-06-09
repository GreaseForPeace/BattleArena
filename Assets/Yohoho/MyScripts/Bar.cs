using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Bar : MonoBehaviour {
  
    public Slider HBar;
    public Slider HBar2;
    public Slider MBar;

    void Start()
    {
        var hero = GetComponent<PlayerClass>();

        HBar.value = hero.CurrHp;
        HBar2.value = hero.CurrHp;
        MBar.value = hero.CurrMp;
		}
	
	// Update is called once per frame
	void Update () {
	
	}
}
