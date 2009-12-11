using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Box : Base
{
    public NetworkView owner;
    [RPC]
    void SetOwner(NetworkMessageInfo a)
    {
        foreach (NetworkView b in this.GetComponents<NetworkView>())
            b.observed = null;
        a.networkView.observed = this.rigidbody;
        owner = a.networkView;
    } 
} 

































