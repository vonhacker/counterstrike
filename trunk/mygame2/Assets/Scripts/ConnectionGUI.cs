using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Threading;
public class Trace : UnityEngine.Debug { }
public class ConnectionGUI : Base
{ 
    

    const int port = 5300;
    public string ip { get { return PlayerPrefs.GetString("ip"); } set { PlayerPrefs.SetString("ip", value); } }
    private void InitServer()
    {
        Network.useNat = false;
        Network.InitializeServer(32, port);
        Connected();
    }
    
    protected override void OnGUI()
    {
        
        if (GUILayout.Button("Active"))  
            Screen.lockCursor = true;
        if (Network.peerType == NetworkPeerType.Disconnected)
        {

            ip = GUILayout.TextField(ip);

            if (GUILayout.Button("Connect"))
                Network.Connect(ip, port);

            if (GUILayout.Button("host"))
                InitServer();
        }
    } 

    protected override void OnConnectedToServer()
    { 
        Connected();                     
    }

    private void Connected()
    {
        
        foreach (GameObject go in FindObjectsOfType(typeof(GameObject)))
            go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);
    }
    protected override void OnApplicationQuit()
    {
        enabled = false;
    }

    protected override void OnDisconnectedFromServer()
    {
        if (enabled)
            Application.LoadLevel(Application.loadedLevel);
    }
    
    


}