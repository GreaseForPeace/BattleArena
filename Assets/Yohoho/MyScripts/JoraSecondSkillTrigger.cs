using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class JoraSecondSkillTrigger : MonoBehaviour
{
    public float Speed;
    private Vector3 _triggerPos;

    void Start()
    {
        //Запоминаем центр появления, хотя это ведь не особо нужно, ведь триггер не двигается
        //но почему бы и нет?
        _triggerPos = gameObject.transform.position;      
    }
	void Update () {
	
	}

    void OnTriggerStay(Collider Si)
    {
        Debug.Log("Эта ересь попалась в гениальную ловушку творца - " + Si.gameObject.name);

        //Проверка: является ли находящийся объект в тригере героем и не Георгием
        if (Si.gameObject.GetComponent<PlayerClass>().enabled)
        {
            //Отключение передвижения врага, во избежании ереси
            if (Si.gameObject.GetComponent<MovePlayer>().enabled)
            {
                Si.gameObject.GetComponent<MovePlayer>().enabled = false;
                Debug.Log(Si.gameObject.GetComponent<PlayerClass>().Name + " не может двигаться");
            }
            //В идеале, оно должно двигать жертву к центру сферы, но об этом потом
            if ((_triggerPos - Si.gameObject.transform.position).magnitude > 1)   
            {
                Debug.Log("Враг по имени" + Si.gameObject.GetComponent<PlayerClass>().Name + " находится на точке " + Si.gameObject.transform.position );
                Debug.Log("Центр сферы там - " + gameObject.transform.position);
              //  Si.gameObject.transform.position = Vector3.Lerp(Si.gameObject.transform.position, _triggerPos, Time.deltaTime * Speed);
                Si.gameObject.transform.Translate((_triggerPos - Si.gameObject.transform.position) * Time.deltaTime * Speed);
            }
        }

    }
}

