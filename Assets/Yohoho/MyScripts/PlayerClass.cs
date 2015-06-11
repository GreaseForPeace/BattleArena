using System;
using UnityEngine;
using System.Collections;

public class PlayerClass : MonoBehaviour
{

    //Integer parameters
    public int CurrHp;      //Current Heal Points of player;
    public int CurrMp;      //Current Mana Points of player;
    public int MaxHp;       //Maximum Heal Points of player;
    public int MaxMp;       //Maximum Mana Points of player;
    public int Armor;       //Defence from physical damage;
    public int MagicResist; //Defence from magical damage;
    public int Damage;      //Hero damage from hand;
    public int MoveSpeed;   //Hero moving speed
    public int TeamNumb;    //Number of player team

    //Boolean parameters
    public bool IsAlive;    //Show alive on died player
    public bool IsInv;      //Show invulnerable player or not
    public bool IsMove;     //Show can moving player or not
    public bool IsStuned;   //Show stunned player or not

    //String parametrs
    public string Name;

    //PURGATORY!
    Vector3 _purgatory = new Vector3(1000, 1000, 1000);

    void OnEnable()
    {
   
    }

    void OnDisable()
    {
     
    }

       //Methods
    public void Die()                  //Calling after hero die
    {
       var hero = GetComponent<PlayerClass>();
       Debug.Log(hero.name + "DIED");
       hero.IsAlive = false;
       gameObject.transform.position = _purgatory;
    }

    public void Attack(PlayerClass enemy)    //Calling when hero deal damage to enemy from hand
    {
        if (enemy.IsAlive)
        {
            DealDamage(Damage, Types.TypesOfDamage.One, enemy);
            var enemyBar = enemy.GetComponent<Bar>().HBar2.value -= Damage;
        }
    }


    public static void DealDamage(int damageValue, Types.TypesOfDamage damageType, PlayerClass enemy)
    {
        enemy.CurrHp -= damageValue;
    }
    
    void Start()
    {
        IsAlive = true;
    }
     
        void Update()
        {
            var hero = GetComponent<PlayerClass>();
            if (hero.CurrHp <= 0)
            {
                hero.Die();
            }
        }
   
}
