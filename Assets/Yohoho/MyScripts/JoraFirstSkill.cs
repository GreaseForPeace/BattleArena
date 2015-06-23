using UnityEngine;
using System.Collections;
using Assets.Yohoho.MyScripts;


public class JoraFirstSkill : BasicSkill {

    private bool _workPlease = false;       //Индикатор прыжка(мол в прыжке или нет)
    private Ray _ray;                       //Луч
    private RaycastHit _hit;                //Точка попадания
    private Vector3 _stopItPlease;          //Сюда будут записываться координаты персонажа перед началом прыжка
    private Vector3 _aveng;                  //Середина между точкой попадания луча и персонажем
    public int Range;
    public float Speed;
    private MovePlayer jorakl;

	// Use this for initialization
	void Start ()
	{
	    IsSlow = true;
	    SlowDuration = 2;
	    IsTransposition = true;

	    Name = "Jora is coming";
	    Type = Types.TypeOnTarget.Point;
	    Damage = 100;
	}
	
	// Update is called once per frame
	void Update () {
	
    if (Input.GetKeyDown(KeyCode.Mouse2) && _workPlease == false)       //Если нажато колесико мыши  и персонаж не в прыжке
    {
        _stopItPlease = gameObject.transform.position;                //Запоминаем позицию персонажа перед началом движения
        _ray = Camera.main.ScreenPointToRay(Input.mousePosition);     //Пускаем луч в позицию мыши
        Physics.Raycast(_ray, out _hit, 1000.0f);
        Debug.Log("Mouse - " + _hit.point);
        Debug.Log("Жорка тут - " + gameObject.transform.position);
        if ((_hit.point - gameObject.transform.position).magnitude <= Range)
        {
            _aveng = (_hit.point + _stopItPlease) / 2f;                    //Получаем середину между Жориком и Точкой попадания луча
            _aveng.y += 5;                                                 //Увеличиваем высоту середины (т.к. Жора должен сначала лететь вверх,а потом спускаться вниз)
            Debug.Log("_aveng - " + _aveng);
            Vector3 moveDirection = _hit.point;
            moveDirection = Camera.main.transform.TransformDirection(moveDirection);
            Vector3 lookDirection = moveDirection + transform.position;
            transform.LookAt(new Vector3(lookDirection.x, transform.position.y, lookDirection.z));
            _workPlease = true;
            jorakl = gameObject.GetComponent<MovePlayer>();
            if (jorakl.enabled)
            {
                jorakl.enabled = false;
            }
            StartCoroutine("Jump");

        }
    }
}

    IEnumerator Jump()
    {
        Debug.Log("Жора летит вверх");
        while ((_aveng - gameObject.transform.position).magnitude > 1f) // Если расстояние больше чем 1  Жора двигается
        {
            gameObject.transform.Translate((_aveng - gameObject.transform.position) * Time.deltaTime * Speed);
            yield return null;
        }
        Debug.Log("Жора на верху");
        Debug.Log("Жора спускается");
        while ((_hit.point - gameObject.transform.position).magnitude > 1f)
        {
            gameObject.transform.Translate((_hit.point - gameObject.transform.position) * Time.deltaTime * Speed);
            yield return null;
        }
        Debug.Log("Жора на месте");
        _workPlease = false;
        if(!jorakl.enabled) {jorakl.enabled = true;}
    }
}
