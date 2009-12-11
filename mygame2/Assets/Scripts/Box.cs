using UnityEngine;
using System.Collections;
public class Box : Base
{
    [RPC]
    void SetScopeTrue(NetworkMessageInfo info)
    {
        if(!Network.isServer) Trace.LogError(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>Set SCope By SErveR?" + Network.isServer + (info.sender == Network.player));
        foreach (NetworkPlayer p in Network.connections)
            info.networkView.SetScope(p, true);        
    }
    [RPC]
    void SetScopeFalse(NetworkMessageInfo info)
    {

        foreach (NetworkPlayer p in Network.connections)
            info.networkView.SetScope(p, false);
    }

    [RPC]
    public void SetOwner(NetworkPlayer np)
    {

        CallLast(Group.SetOwner, "SetOwner", np);
        OwnerID = np;        
    }



    protected override void OnPlayerConnected(NetworkPlayer player)
    {
        foreach (NetworkView nw in this.GetComponents<NetworkView>())
            if (!nw.isMine)
                nw.SetScope(player, false);
    }

    
} 

































