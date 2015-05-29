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

    //Boolean parameters
    public bool IsAlive;    //Show alive on died player
    public bool IsInv;      //Show invulnerable player or not
    public bool IsMove;     //Show can moving player or not
    public bool IsStuned;   //Show stunned player or not

    //Methods
    public void AfterDie()                  //Calling after hero die
    {
        
    }

    public void Attack(GameObject enemy)    //Calling when hero deal damage to enemy from hand
    {
        
    }

    public void Move()
    {
        
    }

    public void TakeDamage(int damageValue, Types.TypesOfDamage damageType)
    {
        
    }

    public void DealDaage(int damageValue, Types.TypesOfDamage damageType)
    {
        
    }
}
