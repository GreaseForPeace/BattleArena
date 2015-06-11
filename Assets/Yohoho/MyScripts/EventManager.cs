using UnityEngine;
using System.Collections;

public class EventManager : MonoBehaviour
{

    public delegate void ClickAction();
    public static event ClickAction Died;

  

    void Update()
    {

    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width / 2 - 50, 20, 100, 30), "Die"))
        {
            var hero = GetComponentInChildren<PlayerClass>();
            hero.CurrHp -= 25;
            Debug.Log("Hero - " + hero.Name + " HP - " + hero.CurrHp);
        }
    }
}
