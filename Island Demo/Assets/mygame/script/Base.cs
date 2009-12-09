using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Reflection;


    public class Base : MonoBehaviour
    {
        public NetworkPlayer? OwnerID;
        protected virtual void Start() { }
        protected virtual void FixedUpdate() { }
        protected virtual void Update() { }
        protected virtual void OnNetworkLoadedLevel() { }
        protected virtual void OnGUI() { }
        protected virtual void OnConnectedToServer() { }
        protected virtual void OnDisconnectedFromServer() { }
        protected virtual void Awake() { }
        protected virtual void OnApplicationQuit() { }
        protected virtual void OnPlayerDisconnected(NetworkPlayer player) { }
        public virtual void OnSetID() { }
        public bool IsMine { get { return networkView.isMine; } }

        
        public void NetworkInstantiate(Object prefab, Vector3 position, Quaternion rotation, Group group)
        {
            Transform t = (Transform)Network.Instantiate(prefab, position, rotation, (int)group);
            t.networkView.group = (int)group;
            t.networkView.RPC("SetID", RPCMode.AllBuffered, Network.player);
        }
        
        [RPC]
        void SetID(NetworkPlayer player)
        {
            
            foreach (Base a in GetComponentsInChildren(typeof(Base)))
            {                
                a.OwnerID = player;
                a.OnSetID();
            }
        }

        

        public void RPC(Group group, RPCMode mode, string fc,params object [] obs)
        {
            if (networkView.isMine)
            {
                networkView.group = (int)group;
                networkView.RPC(fc, mode, obs);
            }
        }
        public void Call(string fc,params object[] obs)
        {
            if (networkView.isMine)
                networkView.RPC(fc, RPCMode.Others, obs);
        }
        public void CallLast(Group group,string fc,params object[] prs)
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
        
        
    }


    //public void NetworkInstanciate(string prehab,Vector3 position, Quaternion rotation, int group)
    //{
    //    Transform clone = (Transform)Network.Instantiate(ToObj(prehab), position, rotation, group);
    //    clone.networkView.group = group;
    //    clone.networkView.RPC("SetOwnerID", RPCMode.AllBuffered, Network.player);            
    //}
    //[RPC]
    //void SetOwnerID(NetworkPlayer player)
    //{
    //    foreach (Base a in GetComponents(typeof(Base)))
    //    {
    //        ////Debug.Log(a);
    //        a.OwnerID = player;
    //        a.onNwInit();
    //    }
    //}



