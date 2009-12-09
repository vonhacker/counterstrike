using UnityEngine;
using System.Collections;
using doru;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

public class SpawnPrefab : Base
{

    
    public static SpawnPrefab _This;

    protected override void Awake()
    {
        _This = this;        
    }
    protected override void Start()
    {
        
    }    
    public Transform _PLView;
    public Transform _PL;
    protected override void OnNetworkLoadedLevel()
    {
        GameObject[] cts = GameObject.FindGameObjectsWithTag("Rig");
        for (int i = 0; i < cts.Length; i++)
        {
            GameObject a = cts[i];
            a.name += i;            
        }


        NetworkInstantiate(_PLView, Vector3.zero, Quaternion.identity, Group.PLView);
        
        Vector3 t = SpawnPoint();
        this.NetworkInstantiate(_PL, t, Quaternion.identity, Group.Player);

    }
    public TimerA _TimerA = new TimerA();
    protected override void Update()
    {
        Thread.Sleep(20);
        _TimerA.Update();
    }
    PL pl { get { return PL._LocalPlayer; } }

    

    public Vector3 SpawnPoint()
    {
        return transform.GetChild(Random.Range(0, transform.childCount)).transform.position;
        
    }
    
    

    
    protected override void OnPlayerDisconnected(NetworkPlayer player)
    {      

        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    }

}
//void OnPlayerConnected(NetworkPlayer player)
//{
//    Create(player, "_PL", "Player");
//    Create(player, "_PLView", "PLView");

//}

//private NetworkPlayer Create(NetworkPlayer player, string field, string tag)
//{
//    foreach (GameObject a in GameObject.FindGameObjectsWithTag(tag))
//    {
//        networkView.RPC("SpawnBox", player, ToID(field), a.networkView.viewID, a.transform.position, a.transform.rotation, a.networkView.owner);
//    }
//    return player;
//}

//using UnityEngine;
//using System.Collections;
//using doru;
//using System.Collections.Generic;
//using System.Reflection;
//using System.Threading;

//public class SpawnPrefab : Base
//{


//    public static SpawnPrefab _This;

//    public SpawnPrefab()
//    {

//    }
//    protected override void Start()
//    {        
//        _This = this;        
//    }    
//    public Transform _PLView;
//    public Transform _PL;
//    public Transform _Patron;
//    void OnNetworkLoadedLevel()
//    {
//        NetworkInstanciate("_PLView", Vector3.zero, Quaternion.identity, (int)Group.PLView, true); 
//        Spawn();
//    }
//    public TimerA _TimerA = new TimerA();
//    public void Update()
//    {
//        Thread.Sleep(20);
//        _TimerA.Update();
//    }
//    PL pl { get { return PL._This; } }



//    public void Spawn()
//    {
//        Transform t = transform.GetChild(Random.Range(0, transform.childCount));
//        NetworkInstanciate("_PL", t.position, t.rotation, (int)Group.Player, true);
//    }

//    public void NetworkInstanciate(string prehab, Vector3 position, Quaternion rotation, int group, bool buffered)
//    {
//        if (buffered)
//        {
//            NetworkViewID viewID = Network.AllocateViewID();
//            networkView.group = group;  
//            networkView.RPC("SpawnBox", RPCMode.AllBuffered, ToID(prehab), viewID, position, rotation, Network.player);
//        }
//        else
//            networkView.RPC("SpawnUnbuffered", RPCMode.All, ToID(prehab), position, rotation, Network.player);

//    }
//    [RPC]
//    void SpawnUnbuffered(int handle, Vector3 position, Quaternion rotation, NetworkPlayer player)
//    {
//        Transform t = ToField(handle);
//        Transform clone = (Transform)Instantiate(t, position, Quaternion.identity);        
//        clone.transform.position = position;
//        clone.transform.rotation = rotation;
//        foreach (Base a in clone.GetComponents(typeof(Base)))
//        {
//            a.OwnerID = player;
//            a.onNwInit();
//        }
//    }
    



//    public int ToID(string s)
//    {
//        int i = 0;
//        FieldInfo b = this.GetType().GetField(s);
//       // if (b == null) ////Debug.Log(s + "filend notfound");
//        foreach (FieldInfo a in this.GetType().GetFields())
//        {

//            if (a.FieldHandle == b.FieldHandle)
//            {
//                return i;
//            }
//            i++;
//        }
//        throw new System.Exception("error not field");
//    }
//    public Transform ToField(int i)
//    {
//        return (Transform)this.GetType().GetFields()[i].GetValue(this);
//    }


//    void OnPlayerDisconnected(NetworkPlayer player)
//    {      
//        Network.RemoveRPCs(player);
//        Network.DestroyPlayerObjects(player);
//    }

//}

//Transform ToObj(string s)
//{
//    //Debug.Log(this.GetType().GetField(s).GetValue(this));
//    return (Transform)this.GetType().GetField(s).GetValue(this);
//}


//Transform t = (Transform)Network.Instantiate(ToObj(prehab), position, rotation, group);
//t.networkView.RPC("SetID", RPCMode.AllBuffered, Network.player);

//public void NetworkInstanciate(string prehab, Vector3 position, Quaternion rotation, int group, bool buffered)
//{
//    if (buffered)
//    {

//        NetworkViewID viewID = Network.AllocateViewID();
//        networkView.group = group;
//        networkView.RPC("SpawnBox", RPCMode.AllBuffered, ToID(prehab), viewID, position, rotation, Network.player);
//    }
//    else
//        networkView.RPC("SpawnUnbuffered", RPCMode.All, ToID(prehab), position, rotation, Network.player);
//}


//[RPC]
//void SpawnUnbuffered(int handle, Vector3 position, Quaternion rotation, NetworkPlayer player)
//{
//    Transform t = ToField(handle);
//    Transform clone = (Transform)Instantiate(t, position, Quaternion.identity);        
//    clone.transform.position = position;
//    clone.transform.rotation = rotation;
//    foreach (Base a in clone.GetComponents(typeof(Base)))
//    {
//        a.OwnerID = player;
//        a.onNwInit();
//    }
//}

//[RPC]
//void SpawnBox(int handle, NetworkViewID viewID, Vector3 position, Quaternion rotation, NetworkPlayer player)
//{
//    Transform t = ToField(handle);
//    Transform clone = (Transform)Instantiate(t, position, Quaternion.identity);        
//    foreach (NetworkView nw in clone.GetComponentsInChildren<NetworkView>())
//    {
//        nw.networkView.viewID = viewID;
//    }
//    clone.transform.position = position;
//    clone.transform.rotation = rotation;
//    foreach (Base a in clone.GetComponents(typeof(Base)))
//    {
//        a.OwnerID = player;
//        a.onNwInit();
//    }
//}



//public int ToID(string s)
//{
//    int i = 0;
//    FieldInfo b = this.GetType().GetField(s);
//   // if (b == null) ////Debug.Log(s + "filend notfound");
//    foreach (FieldInfo a in this.GetType().GetFields())
//    {

//        if (a.FieldHandle == b.FieldHandle)
//        {
//            return i;
//        }
//        i++;
//    }
//    throw new System.Exception("error not field");
//}
//public Transform ToField(int i)
//{
//    return (Transform)this.GetType().GetFields()[i].GetValue(this);
//}
//else
//    if (a.networkView.isMine && a.networkView.viewID != NetworkViewID.unassigned) AssignID(i, NetworkViewID.unassigned);