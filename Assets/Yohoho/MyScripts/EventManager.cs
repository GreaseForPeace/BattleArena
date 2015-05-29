using UnityEngine;
using System.Collections;

public class EventManager : MonoBehaviour
{

    public delegate void ClickAction();
    public static event ClickAction OnClicked;

    void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width / 2 - 50, 20, 100, 30), "Die"))
        {
            if (OnClicked != null)
                OnClicked();
        }
    }
}
