using UnityEngine;
using System.Collections;
using System.Security.Cryptography.X509Certificates;

public class RTSCamera : MonoBehaviour
{

    public float CamSpeed = 1;
    public int GUISize = 25;
    public Transform target;

    //zoom camera
    public float distance = 60;
    public float dumping = 4;
    public float senetivytiDistance = 50;
    public float minFOV = 40;
    public float maxFOV = 60;


    void Start()
    {
        transform.position = new Vector3(target.position.x, transform.position.y, target.position.z - 5);
    }
    void Update()
    {
        var recdown = new Rect(0, 0, Screen.width, GUISize);

        var recup = new Rect(0, Screen.height - GUISize, Screen.width, GUISize);

        var recleft = new Rect(0, 0, GUISize, Screen.height);

        var recright = new Rect(Screen.width - GUISize, 0, GUISize, Screen.height);

        if (recdown.Contains(Input.mousePosition))
        {
            transform.Translate(0, 0, -CamSpeed, Space.World);
        }

        if (recup.Contains(Input.mousePosition))
        {
            transform.Translate(0, 0, CamSpeed, Space.World);
        }

        if (recleft.Contains(Input.mousePosition))
        {
            transform.Translate(-CamSpeed, 0, 0, Space.World);
        }

        if (recright.Contains(Input.mousePosition))
        {
            transform.Translate(CamSpeed, 0, 0, Space.World);
        }

        // Set camera to player
        if (Input.GetKeyDown("f1"))
        {
            transform.position = new Vector3(target.position.x, transform.position.y, target.position.z - 5);
        }

        //zoom camera
        distance -= Input.GetAxis("Mouse ScrollWheel")*senetivytiDistance;
        distance = Mathf.Clamp(distance, minFOV, maxFOV);
        camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, distance, Time.deltaTime*dumping);
    }
}