using UnityEngine;
using System.Collections;

public class Die : MonoBehaviour {

    void OnEnable()
    {
        EventManager.OnClicked += Dying;
    }

    void OnDisable()
    {
        EventManager.OnClicked -= Dying;
    }

    public void Dying()
    {
        var hero = gameObject.GetComponent<PlayerClass>();
        var slider = gameObject.GetComponent<Bar>();
        hero.CurrHp -= 25;
        slider.HBar2.value -= 25;
        Debug.Log("Jora hp - " + hero.CurrHp);
        Debug.Log("-25");
    }

}
