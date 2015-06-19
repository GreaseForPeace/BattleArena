using UnityEngine;
using System.Collections;

public class UnitController : MonoBehaviour
{

    private Vector3 _point1, _point2;
    private int LayerMaskDefault = 1 << 0;
    private float height, width;
    private Rect _selectRect;
    public Texture2D SelectTexture;
    private bool DragMouse = false;

	// Use this for initialization
	void Start ()
	{

	}
	
	// Update is called once per frame
	void Update () {

	    if (Input.GetKeyDown(KeyCode.Mouse0))
	    {
	        _point1 = Input.mousePosition;
	        
	    }

	    if (Input.GetKey(KeyCode.Mouse0))
	    {
            _point2 = Input.mousePosition;
            DragMouse = true;
	    }

	    if (Input.GetKeyUp(KeyCode.Mouse0))
	    {
	        
            DragMouse = false;
	    }
	}

    void OnGUI()
    {
        if (DragMouse)
        {
            width = _point2.x - _point1.x;
            height = (Screen.height - _point2.y) - (Screen.height - _point1.y);
            _selectRect = new Rect(_point1.x, Screen.height - _point1.y, width, height);
            GUI.DrawTexture(_selectRect, SelectTexture, ScaleMode.StretchToFill, true);
        }
    }
}
