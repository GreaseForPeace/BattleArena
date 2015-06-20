using UnityEngine;
using System.Collections;

public class EventManager : MonoBehaviour
{
    Transform trans;

    void Update()
    {

    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width / 2 - 50, 20, 100, 30), "Die"))
        {
            var hero = GetComponentInChildren<PlayerClass>();
            hero.CurrHp -= hero.Damage;
            Debug.Log("Hero - " + hero.Name + " HP - " + hero.CurrHp);
            Debug.Log("PURGATORY HERE - " + hero.Purgatory);
        }

        if (GUI.Button(new Rect(Screen.width / 3 - 50, 20, 100, 30), "Respawn"))
        {
            var hero = GetComponentInChildren<PlayerClass>();
            hero.Spawn(new Vector3(17, 5, 428));
        }
    }
}
