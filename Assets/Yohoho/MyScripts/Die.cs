using UnityEngine;
using System.Collections;

public class Die : MonoBehaviour {

    void OnEnable()
    {
        EventManager.OnClicked += Dying;
    }

    void OnDisable()
    {
        EventManager.OnClicked -= Dying;
    }

    public void Dying()
    {
        Debug.Log("Die working");
    }

}
