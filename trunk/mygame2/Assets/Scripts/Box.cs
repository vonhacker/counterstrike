using UnityEngine;
using System.Collections;
public class Box : Base
{
    [RPC]
    void SetScopeTrue(NetworkMessageInfo info)
    {           
        foreach (NetworkPlayer p in Network.connections)
            info.networkView.SetScope(p, true);
    }

    protected override void OnPlayerConnected(NetworkPlayer player)
    {
        foreach (NetworkView nw in this.GetComponents<NetworkView>())
            if (!nw.isMine)
                nw.SetScope(player, false);
    }

    [RPC]
    void SetScopeFalse(NetworkMessageInfo info)
    {
        
        foreach (NetworkPlayer p in Network.connections)
            info.networkView.SetScope(p, false);
    }
} 

































