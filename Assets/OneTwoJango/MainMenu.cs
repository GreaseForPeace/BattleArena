using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    public Button NewGame;
    public Button Options;
    public Button Exit;

    //Меню
    public bool Play = false;
    public bool Settings = false;
    public bool Author = false;
    public bool Quit = false;

    //Камеры
    public Camera camera1;
    public Camera camera2;
    public Camera camera3;

    //Меню настроек
    public bool Back;

    //Гафика
    public bool Low = false;
    public bool Medium = false;
    public bool High = false;
    public bool Fantastic = false;

    public bool BackM = false;

	// Use this for initialization
	void Start ()
	{
	    
	}
	
	// Update is called once per frame
	void Update () {

	    if (Play)
	    {
	        Application.LoadLevel(1);
	    }
	    if (Options)
	    {
	        camera1.enabled = false;
	        camera2.enabled = true;
	    }
	    if (Author)
	    {
	        camera1.enabled = false;
	        camera3.enabled = true;
	    }
	    if (Quit)
	    {
	        Application.Quit();
	    }

        //========================
	    if (Back)
	    {
	        camera1.enabled = true;
	        camera2.enabled = false;
	    }
        if (Low)
        {
            QualitySettings.currentLevel = QualityLevel.Simple;
        }
        if (Medium)
        {
            QualitySettings.currentLevel = QualityLevel.Good;
        }
        if (High)
        {
            QualitySettings.currentLevel = QualityLevel.Beautiful;
        }
        if (Fantastic)
        {
            QualitySettings.currentLevel = QualityLevel.Fantastic;
        }
        
        //====================
        if (BackM)
        {
            Application.LoadLevel(0);
        }
	}
    void OnGUI()
    {
        if (Options)
        {
            camera1.enabled = false;
            camera2.enabled = true;
        }
    }
}
