using UnityEngine;
using System.Collections;
using System;

public class Base : MonoBehaviour
{
        
    // Use this for initialization
    protected virtual void OnPlayerConnected(NetworkPlayer player) { }
    protected virtual void OnLevelWasLoaded(int level) { }
    protected virtual void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) { }
    protected virtual void Start() { }
    protected virtual void FixedUpdate() { }
    protected virtual void Update() { }

    protected virtual void OnNetworkLoadedLevel() { }
    protected virtual void OnGUI() { }
    protected virtual void OnConnectedToServer() { }
    protected virtual void OnDisconnectedFromServer() { }
    protected virtual void Awake() { }
    protected virtual void OnApplicationQuit(){}
    protected virtual void OnPlayerDisconnected(NetworkPlayer player) { }
    protected virtual void OnCollisionEnter(Collision collisionInfo) { }
    protected virtual void OnNetworkInstantiate(NetworkMessageInfo info) { }
    public virtual void OnSetID() { }
    public bool isMine { get { return networkView.isMine; } }  
      
    public void RPC(Group group, RPCMode mode, string fc, params object[] obs)
    {
        if (networkView.isMine)
        {
            networkView.group = (int)group;
            networkView.RPC(fc, mode, obs);
        }
    }
    public void Call(string fc, params object[] obs)   
    {
        if (networkView.isMine)
            networkView.RPC(fc, RPCMode.Others, obs);
    }
    public void CallLast(Group group, string fc, params object[] prs)
    {
               
        if (networkView.isMine)              
        {
            networkView.group = (int)group;
            Network.RemoveRPCs(networkView.owner, (int)group);
            networkView.RPC(fc, RPCMode.OthersBuffered, prs);
        }
    }
    public void CallBuffered(Group group, string fc, params object[] prs)
    {
        if (networkView.isMine)
        {
            networkView.group = (int)group;
            networkView.RPC(fc, RPCMode.OthersBuffered, prs);
        }
    }

    public static T Find<T>(string s) where T : Component 
    {
        GameObject g = GameObject.Find(s);
        if (g != null) return g.GetComponent<T>();
        return null;
    }

    public static T Find<T> () where T : Component
    {        
        return (T)Component.FindObjectOfType(typeof(T));
    }
    public NetworkPlayer? OwnerID
    {
        get { return _OwnerID; }
        set { _OwnerID = value; pwnerID2 = value.HasValue ? value.Value.GetHashCode() : -2; }
    }
    NetworkPlayer? _OwnerID;
    public int pwnerID2 = -2;        
    
}
