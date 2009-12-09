
using UnityEngine;
using System.Collections;
using System;

using System.Runtime.CompilerServices;
using doru;
using System.Collections.Generic;


public class PLView : Base
{
    public static PLView _LocalPLView;
    public static Dictionary<NetworkPlayer,PLView> pls = new Dictionary<NetworkPlayer,PLView>();
    public int score;
    public string Nick;
    
    protected override void Awake()
    {
        base.Awake();
    }
    
    public override void OnSetID()
    {
        //Trace.Log("PlViewOnInit");
        enabled = networkView.isMine;
        pls.Add(OwnerID.Value, this);
    }
    
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        stream.Serialize(ref score);        
    }
    protected override void Start()
    {
        //Trace.Log("Start owner" + OwnerID);
        _LocalPLView = this;        
        networkView.RPC("SetNick", RPCMode.AllBuffered, ConnectGui._This.Nick);        
    }
    [RPC]
    public void SetNick(string s)
    {
        //Trace.Log((Network.player == OwnerID) + "" + OwnerID + "set nick:" + s);
        Nick = s;
    }
    [RPC]
    public void AddPoint(int s)
    {
        score += s;
    }

}
