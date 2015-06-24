/// <summary>
/// Move player.
/// Управление мышью персонажем
/// Вещается на персонажа
/// </summary>
using UnityEngine;
using System.Collections;

public class MovePlayer : MonoBehaviour
{
    
    public float stopStart = 1.5f, speed = 5f, rotationSpeed = 100f, heightPlayer = 1f;

    private float mag, angleToTarget;
    private Ray ray;
    private RaycastHit hit;
    private Vector3 dir;
    private Vector3 target = new Vector3();
    private Vector3 lastTarget = new Vector3();
    private PlayerClass hero;

    public AnimationClip a_Idle;
    public float a_IdleSpeed = 1;
    public AnimationClip a_Walk;
    public float a_WalkSpeed = 2;
    public AnimationClip a_Death;
    //public Camera camera;

    public bool walk;

    private void Start()
    {
        animation[a_Idle.name].speed = a_IdleSpeed;
        animation[a_Walk.name].speed = a_WalkSpeed;
        animation.CrossFade(a_Idle.name);
        hero = gameObject.GetComponent<PlayerClass>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))                               //ПКМ
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);        //Цель обнаружена
            if (Physics.Raycast(ray, out hit, 10000.0f))                    //ОГОНЬ!
            {
                target = hit.point;                                         //Цель обнаружена
                if (hit.collider.GetComponent<PlayerClass>())               //Проверка цели
                {
                    var enemy = hit.collider.GetComponent<PlayerClass>();   //Враг обнаружен
                    if (enemy.TeamNumb != hero.TeamNumb)
                    {
                        Debug.Log("It's fucking enemy " + enemy.Name);
                        if (hero.AttakRange >= (target - hero.gameObject.transform.position).magnitude)
                        {
                            hero.Attack(enemy);
                            Debug.Log(hero.Name + " - " + hero.CurrHp);
                            Debug.Log("JoraTwo hp - " + enemy.CurrHp);
                        }

                    };
                }
                else                                                        //Ложная тревога
                {
                    var enemy = hit.collider.gameObject.name;
                    Debug.Log("You clicked on " + enemy);
                }
            }
        }
        LookAtThis();                                                       //Смотри
        MoveTo(hero.AttakRange);                                                           //и беги
    }  
    private void CalculateAngle(Vector3 temp)
    {
        dir = new Vector3(temp.x, transform.position.y, temp.z) - transform.position;
        angleToTarget = Vector3.Angle(dir, transform.forward);
    }

    private void LookAtThis()
    {
        if (target != lastTarget)
        {
            CalculateAngle(target);
            if (angleToTarget > 3)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), rotationSpeed * UnityEngine.Time.deltaTime * 4f);
        }
    }

    private void MoveTo(float attackRange)
    {
        if (target != lastTarget)
        {
            if ((transform.position - target).sqrMagnitude > heightPlayer + attackRange)
            {
                if (!walk)
                {
                    animation.CrossFade(a_Walk.name);
                    walk = true;
                }
                mag = (transform.position - target).magnitude;
                transform.position = Vector3.MoveTowards(transform.position, target, mag > stopStart ? speed * UnityEngine.Time.deltaTime : Mathf.Lerp(speed * 0.5f, speed, mag / stopStart) * UnityEngine.Time.deltaTime);
                ray = new Ray(transform.position, -Vector3.up);
                if (Physics.Raycast(ray, out hit, 1000.0f))
                {
                    transform.position = new Vector3(transform.position.x, hit.point.y + heightPlayer, transform.position.z);
                }
            }
            else
            {
                lastTarget = target;
                if (walk)
                {
                    animation.CrossFade(a_Idle.name);
                    walk = false;
                }
            }
        }
    }
}
