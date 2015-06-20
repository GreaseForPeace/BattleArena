using UnityEngine;
using System.Collections;

namespace Assets.Yohoho.MyScripts
{

    public class BasicSkill : MonoBehaviour
    {
        public string Name = "BasicSkillName";
        public Types.TypeOnTarget Type;
        public int Damage = 0;
        public int StunDuration = 0;
        public int SlowDuration = 0;
        public int DurableDuration = 0;

        public bool IsDamage = false;
        public bool IsStun = false;
        public bool IsSlow = false;
        public bool IsDurable = false;
        public bool IsTransposition = false;
        public bool IsDefense = false;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
}

}
