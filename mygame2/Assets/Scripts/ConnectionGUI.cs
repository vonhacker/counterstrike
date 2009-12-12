using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Threading;
public class Trace : UnityEngine.Debug { }
public class ConnectionGUI : Base
{
    
    public string Nick { get { return PlayerPrefs.GetString("Nick"); } set { PlayerPrefs.SetString("Nick",value); } }
    const int port = 5300;
    public string ip { get { return PlayerPrefs.GetString("ip"); } set { PlayerPrefs.SetString("ip", value); } }
    private void InitServer()
    {
        Network.useNat = false;
        Network.InitializeServer(32, port);
        LoadLevel(0);
    }
    
    protected override void OnGUI()
    {        

        if (GUILayout.Button("Active"))  
            Screen.lockCursor = true;
        if (Network.peerType == NetworkPeerType.Disconnected)
        {            

            ip = GUILayout.TextField(ip);

            if (GUILayout.Button("Connect") && Nick.Length > 0)
            {                
                Network.Connect(ip, port);
            }
            Nick = GUILayout.TextField(Nick);

            if (GUILayout.Button("host") && Nick.Length > 0)
                InitServer();
        }
    }

    private int lastLevelPrefix = 0;
    
    protected override void OnApplicationQuit()
    {
        enabled = false;
        
    }


    protected override void OnPlayerConnected(NetworkPlayer player)
    {
        networkView.RPC("LoadLevel", RPCMode.All, lastLevelPrefix + 1);
    }
    [RPC]
    private void LoadLevel(int levelPrefix)
    {
        Trace.Log(">>>>>>>>>>>>>>>>>>>>>>>>loading level");
        //lastLevelPrefix = levelPrefix;
        //Network.SetSendingEnabled(0, false);	
        Network.isMessageQueueRunning = false;
        //Network.SetLevelPrefix(levelPrefix);
        Application.LoadLevel("mygame2");                
    }
    protected override void OnLevelWasLoaded(int level)
    {
        Trace.Log(">>>>>>>>>>>>>>>>>>>>>>>>level loaded");
        Network.isMessageQueueRunning = true;
        //Network.SetSendingEnabled(0, true);
    }
}
//protected override void OnDisconnectedFromServer()
//{
//    if (enabled)
//        Application.LoadLevel(Application.loadedLevel);
//}