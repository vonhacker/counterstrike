using UnityEngine;
using System.Collections;
public class Trace : Debug { }
public class LGUI : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }
    public string life { get { return "" + (PL._LocalPlayer == null ? 0 : (PL._LocalPlayer).Life); } }
    void OnGUI()
    {

        if (Network.peerType != NetworkPeerType.Disconnected && Input.GetKey(KeyCode.Tab))
        {

            Vector2 v = new Vector2(500, 300) / 2;
            Rect r = new Rect((Screen.width / 2) - v.x, (Screen.height / 2) - v.y, v.x * 2, v.y * 2);
            GUI.Box(r, "Group is here");
            GUILayout.BeginArea(r);
            {
                GUILayout.Space(20);
                foreach (PLView a in FindObjectsOfType(typeof(PLView)))
                    GUILayout.Label(a.networkView.isMine + ":" + a.OwnerID + "," + a.networkView.viewID.owner + ":" + a.Nick + " Score:" + a.score +
                        "    Ping:" + Network.GetLastPing(a.OwnerID.Value) +
                        "   IPAddress:" + a.OwnerID.Value.ipAddress + "port:" + a.OwnerID.Value.port);
                GUILayout.Label("<<<<Debug>>>>");
                foreach (NetworkPlayer a in Network.connections)
                    GUILayout.Label(a.ipAddress + " " + a.port + " " + a);
                foreach (PL a in FindObjectsOfType(typeof(PL)))
                {
                    GUILayout.Label(a.OwnerID + "");
                }
            }
            GUILayout.EndArea();
        }
        GUILayout.Label(life);
    }

    private static NetworkPlayer GetInfo(NetworkPlayer np)
    {
        
        

        return np;
    }
    // Update is called once per frame
    void Update()
    {                
        //Debug.DrawLine(Vector3.zero, new Vector3(100, 0, 0), Color.red);
    }

}
