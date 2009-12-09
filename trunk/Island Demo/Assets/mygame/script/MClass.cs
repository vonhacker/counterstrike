using UnityEngine;
public class MClass : MonoBehaviour
{

    
    void OnFailedToConnectToMasterServer(NetworkConnectionError info)
    {
        Log(info);
    }

    void OnFailedToConnect(NetworkConnectionError info)
    {
        Log(info.ToString());
    }
    void Log(object o)
    {
        info = o.ToString();
    }
    void OnGUI()
    {
        if (Network.peerType == NetworkPeerType.Disconnected)
            GUILayout.Window(0, CenterRect(500,300), MakeWindow, "Server Controls");
    }

    void Awake()
    {
        
        MasterServer.RequestHostList(gamename);
        
    }

    private Rect CenterRect(int w , int h)
    {
        Vector2 v = new Vector2(w, h);
        Vector2 c = new Vector2(Screen.width, Screen.height);
        Vector2 u = (c - v) / 2;
        return new Rect(u.x, u.y, v.x, v.y);
    }


    public static string gamename = "Swiborg";
    string info = "";
    void MakeWindow(int id)
    {
        
            if (GUILayout.Button("refresh server list"))
                MasterServer.RequestHostList(gamename);
            GUILayout.Label(info);
            HostData[] data = MasterServer.PollHostList();
            // Go through all the hosts in the host list
            foreach (HostData element in data)
            {
                GUILayout.BeginHorizontal();
                string name = element.gameName + " " + element.connectedPlayers + " / " + element.playerLimit;
                GUILayout.Label(name);
                GUILayout.Space(5);

                string hostInfo = "[";
                foreach (string host in element.ip)
                    hostInfo = hostInfo + host + ":" + element.port + " ";
                hostInfo = hostInfo + "]";
                GUILayout.Label(hostInfo);
                GUILayout.Space(5);
                GUILayout.Label(element.comment);
                GUILayout.Space(5);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Connect"))
                {
                    // Set NAT functionality based on the host information
                    Network.useNat = element.useNat;
                    if (Network.useNat)
                        print("Using Nat punchthrough to connect to host");
                    else
                        print("Connecting directly to host");
                    Network.Connect(element.ip, ConnectGui._This.listenPort);
                }
                GUILayout.EndHorizontal();
                
            }
        }

}




//{

//    Rect windowRect = new Rect(20, 20, 120, 50);

//    void OnGUI()
//    {
//        // Register the window. Notice the 3rd parameter 
//        windowRect = GUILayout.Window(0, windowRect, DoMyWindow, "My Window");
//    }

//    // Make the contents of the window
//    void DoMyWindow(int windowID)
//    {
//        // This button will size to fit the window
//        if (GUILayout.Button("Hello World"))
//            print("Got a click");
//    }

//}