using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{

    public float SharpnessZoom;     //Плавность камеры
    public float CameraPosition;    //Точка под камерой
    public int CameraZoomMax;       //Максимальный зум
    public int CameraZoomMin;       //Минимальный зум
    public float CameraSpeed;       //Скорость камеры
    public Transform target;
    private RaycastHit _hit;        //Точка попадания луча



	// Use this for initialization
	void Start () {
        transform.position = new Vector3(target.position.x, target.position.y + CameraPosition, target.position.z -  6f);
	}
	
	// Update is called once per frame
	void Update ()
	{
        if (Input.GetKeyDown("f1"))
        {
            transform.position = new Vector3(target.position.x, target.position.y + CameraPosition, target.position.z - 6f);
        }
	    CameraHeightPosition();
        CameraWidthPosition();
	}

    void CameraHeightPosition()
    {
        Vector3 directionRay = transform.TransformDirection(Vector3.forward);
        if (Physics.Raycast(transform.position, directionRay, out _hit, 100f))
        {
            if (_hit.collider.tag == "Terrain")
            {
                if (_hit.distance < CameraPosition) //Если дистанция до террейна меньше дальности камеры
                {
                    transform.position += new Vector3(0, (CameraPosition - _hit.distance), 0);
                }
                if (_hit.distance > CameraPosition)
                {
                    transform.position -= new Vector3(0, (_hit.distance - CameraPosition), 0);
                }
            }
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0 && CameraPosition < CameraZoomMax)
        {
            CameraPosition += 2*SharpnessZoom*Time.deltaTime;
            CameraSpeed += 0.007f;
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && CameraPosition > CameraZoomMin)
        {
            CameraPosition -= 2*SharpnessZoom*Time.deltaTime;
            CameraSpeed -= 0.007f;
        }
    }

    void CameraWidthPosition()
    {
        if (20 > Input.mousePosition.x)
        {
            transform.position -= new Vector3(CameraSpeed, 0, 0);
        }

        if ((Screen.width - 10) < Input.mousePosition.x)
        {
            transform.position += new Vector3(CameraSpeed, 0 , 0);
        }

        if (20 > Input.mousePosition.y)
        {
            transform.position -= new Vector3(0, 0, CameraSpeed);
        }

        if ((Screen.height - 10) < Input.mousePosition.y)
        {
            transform.position += new Vector3(0, 0, CameraSpeed);
        }
    }
}
