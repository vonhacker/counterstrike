using UnityEngine;
using System.Collections;

public class ConnectGui : Base
{
    public static ConnectGui _This;
    public string remoteIP = "127.0.0.1";
    public int remotePort = 25000;
    public int listenPort = 25000;
    public string Nick { get { return PlayerPrefs.GetString("nick"); } set { PlayerPrefs.SetString("nick", value); } }
    public bool useNAT = false;
    protected override void Awake()
    {
        //Nick = "";
        _This = this;
        //if (FindObjectOfType(typeof(ConnectGuiMasterServer)))
        //    this.enabled = false;
        //if(Debug.isDebugBuild)            
        //    InitServer();
    }
    
    string nick="";
    protected override void OnGUI()
    {
        
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        

        if (Network.peerType == NetworkPeerType.Disconnected)
        {            
            GUILayout.BeginVertical();
            
            if (Nick == "")
            {
                GUILayout.Label("nick:");
                
                nick = GUILayout.TextField(nick, GUILayout.MinWidth(100));
                if (GUILayout.Button("Set Nick") && nick != "") Nick = nick;
            }
            else
            {
                if (GUILayout.Button("Connect"))
                {
                    Network.useNat = useNAT;
                    
                    Network.Connect(remoteIP, remotePort);                    
                }
                if (GUILayout.Button("Start Server"))
                {
                    //Network.useNat = !Network.HavePublicAddress();
                   // Network.InitializeServer(32, 25000);                    
                    
                       
                    InitServer();
                    Screen.lockCursor = true;
                }
                GUILayout.EndVertical();
                remoteIP = GUILayout.TextField(remoteIP, GUILayout.MinWidth(100));
                remotePort = int.Parse(GUILayout.TextField(remotePort.ToString()));
            }
        }
        else    
        {

            if (GUILayout.Button("Disconnect")) 
            {
                Network.Disconnect(200);
            }
           
        }
        if (GUILayout.Button("Active")) Screen.lockCursor = true;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void InitServer()
    {        
        Network.useNat = useNAT;         
        Network.InitializeServer(32, listenPort);
        MasterServer.RegisterHost(MClass.gamename , Nick + "s game", "Test Discription");
        foreach (GameObject go in FindObjectsOfType(typeof(GameObject)))
            go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);
    }

    protected override void OnApplicationQuit()
    {
        enabled = false;
    }

    protected override void OnConnectedToServer()
    {
        foreach (GameObject go in FindObjectsOfType(typeof(GameObject)))
            go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);
    }

    protected override void OnDisconnectedFromServer()
    {        
        if (this.enabled != false)
            Application.LoadLevel(Application.loadedLevel);
        //else
        //    FindObjectOfType(typeof(NetworkLevelLoad)).OnDisconnectedFromServer();
        PLView.pls.Clear();
    }

}
