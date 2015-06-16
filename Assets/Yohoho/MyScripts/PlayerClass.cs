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
    public Vector3 Purgatory = new Vector3(1000, 1000, 1000);

    //CABOOM
    public GameObject CABOOM; 

    void OnEnable()
    {
        Debug.Log("PURGATORY HERE - " + Purgatory);
    }

    void OnDisable()
    {
     
    }

       //Methods
    public void Die()                  //Calling after hero die
    {
       var hero = GetComponent<PlayerClass>();     //Big dady found you
       Debug.Log(hero.name + " DIED");             //You died lol
       hero.IsAlive = false;                       //Yeap, you rly died
       Instantiate(CABOOM, gameObject.transform.position, gameObject.transform.rotation);
       gameObject.transform.position = Purgatory; //GO TO PURGATORY, NIGGER!
       
    }

    public void Attack(PlayerClass enemy)    //Calling when hero deal damage to enemy from hand
    {
        if (enemy.IsAlive && IsAlive)
        {
            DealDamage(Damage, Types.TypesOfDamage.One, enemy);
        }
    }


    public void DealDamage(int damageValue, Types.TypesOfDamage damageType, PlayerClass enemy)
    {
        enemy.CurrHp -= damageValue;
    }

    public void Spawn(Vector3 pos)
    {
        if (!IsAlive)
        {
            gameObject.transform.position = pos;
            CurrHp = MaxHp;
            IsAlive = true;
        }
    }

    void Start()
    {
        IsAlive = true;
    }
     
        void Update()
        {
            var hero = GetComponent<PlayerClass>();
            if (hero.CurrHp <= 0 && hero.IsAlive)
            {
                hero.Die();
            }
        }
   
}
