using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class GameManagerVik : Photon.MonoBehaviour {

    // this is a object name (must be in any Resources folder) of the prefab to spawn as player avatar.
    // read the documentation for info how to spawn dynamically loaded game objects at runtime (not using Resources folders)
    public string playerPrefabName = "Charprefab";
    public Canvas FUUUUUCK;
    public Slider HealthBar;
    public Slider ManaBar;
    public Camera Minimap;

    void OnJoinedRoom()
    {
        StartGame();
    }
    
    IEnumerator OnLeftRoom()
    {
        //Easy way to reset the level: Otherwise we'd manually reset the camera

        //Wait untill Photon is properly disconnected (empty room, and connected back to main server)
        while(PhotonNetwork.room!=null || PhotonNetwork.connected==false)
            yield return 0;

        Application.LoadLevel(Application.loadedLevel);

    }

    void StartGame()
    {
        Camera.main.farClipPlane = 1000; //Main menu set this to 0.4 for a nicer BG    

        //prepare instantiation data for the viking: Randomly diable the axe and/or shield
        bool[] enabledRenderers = new bool[2];
        enabledRenderers[0] = Random.Range(0,2)==0;//Axe
        enabledRenderers[1] = Random.Range(0, 2) == 0; ;//Shield
        
        object[] objs = new object[1]; // Put our bool data in an object array, to send
        objs[0] = enabledRenderers;

        // Spawn our local player

        var a = PhotonNetwork.Instantiate(this.playerPrefabName, transform.position, Quaternion.identity, 0);
        var player = a.GetComponent<PlayerClass>();
        GameObject fuckingCam = GameObject.Find("Camera");
        CameraController fuckingCamController = fuckingCam.gameObject.GetComponent<CameraController>();
        fuckingCamController.target = a.transform;
        fuckingCamController.DoIT();
        Debug.Log("krgdjgkfjg " + FUUUUUCK.active);
        if (!FUUUUUCK.active)
        {
            FUUUUUCK.active = true;
        }
        if (!Minimap.active) {

            Minimap.active = true;
        }
        HealthBar.maxValue = player.MaxHp;
        HealthBar.value = player.CurrHp;
        ManaBar.maxValue = player.MaxMp;
        ManaBar.value = player.CurrMp;
        
    }

    void OnGUI()
    {
        if (PhotonNetwork.room == null) return; //Only display this GUI when inside a room

        if (GUILayout.Button("Leave Room"))
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    void OnDisconnectedFromPhoton()
    {
        Debug.LogWarning("OnDisconnectedFromPhoton");
    }    
}
