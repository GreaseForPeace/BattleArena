﻿using UnityEngine;
using System.Collections;

public class Skills : MonoBehaviour
{

    private bool _workPlease = false;       //Индикатор прыжка(мол в прыжке или нет)
    private Ray _ray;                       //Луч
    private RaycastHit _hit;                //Точка попадания
    private Vector3 _stopItPlease;          //Сюда будут записываться координаты персонажа перед началом прыжка
    private Vector3 _aveng;                  //Середина между точкой попадания луча и персонажем

	// Use this for initialization
	void Start (){
	}
	
	// Update is called once per frame
	void Update (){

    if (Input.GetKeyDown(KeyCode.Mouse2) && _workPlease == false)       //Если нажато колесико мыши  и персонаж не в прыжке
    {
        _stopItPlease = gameObject.transform.position;                //Запоминаем позицию персонажа перед началом движения
        _ray = Camera.main.ScreenPointToRay(Input.mousePosition);     //Пускаем луч в позицию мыши
        Physics.Raycast(_ray, out _hit, 1000.0f);
        Debug.Log("Mouse - " + _hit.point);
        Debug.Log("Жорка тут - " + gameObject.transform.position);
        if ((_hit.point - gameObject.transform.position).magnitude <= 10)
        {
            _aveng = (_hit.point + _stopItPlease) / 2f;                    //Получаем середину между Жориком и Точкой попадания луча
            _aveng.y += 5;                                                 //Увеличиваем высоту середины (т.к. Жора должен сначала лететь вверх,а потом спускаться вниз)
            Debug.Log("_aveng - " + _aveng);
            _workPlease = true;
            StartCoroutine("Jump");
        }
    }



}
    IEnumerator Jump()
    {
        Debug.Log("Жора летит вверх");
        while ((_aveng - gameObject.transform.position).magnitude > 1f) // Если расстояние больше чем 0.1 Жора двигается
        {
            gameObject.transform.Translate((_aveng - gameObject.transform.position) * Time.deltaTime * 4f);
            yield return null;
        }
        Debug.Log("Жора на верху");
        Debug.Log("Жора спускается");
        while ((_hit.point - gameObject.transform.position).magnitude > 1f)
        {
            gameObject.transform.Translate((_hit.point - gameObject.transform.position) * Time.deltaTime * 4f);
            yield return null;
        }
        Debug.Log("Жора на месте");
        _workPlease = false;
    }
}
