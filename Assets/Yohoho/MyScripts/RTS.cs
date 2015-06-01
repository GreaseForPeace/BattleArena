using UnityEngine;
using System.Collections;

public class RTS : MonoBehaviour
{

    public float HorizontalSpeed = 40;
    public float VerticalSpeed = 40;
    public float CameraRotateSpeed = 80;
    public float CameraDistance = 30;

    //internal check
    private float curDistance;

	// Update is called once per frame
	void Update ()
	{

	    float horizontal = Input.GetAxis("Horizontal") * HorizontalSpeed * Time.deltaTime;
	    float vertical = Input.GetAxis("Vertical") * VerticalSpeed * Time.deltaTime;
	   // float rotation = Input.GetAxis("Rotation");

        transform.Translate(Vector3.forward * vertical);
        transform.Translate(Vector3.right * horizontal);

	   // if (rotation != 0)
	  //  {
	  //      transform.Rotate(Vector3.up, rotation * CameraRotateSpeed * Time.deltaTime, Space.World );
	  //  }

	    RaycastHit hit;

        if(Physics.Raycast(transform.position, -transform.up, out hit, 100.0f))
        {
            curDistance = Vector3.Distance(transform.position, hit.point);
        }

	    if (curDistance != CameraDistance)
	    {
	        float difference = CameraDistance - curDistance;
	        transform.position = Vector3.Lerp(transform.position, transform.position + new Vector3(0, difference, 0),
	            Time.deltaTime);
	    }
	}
}
