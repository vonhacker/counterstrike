using UnityEngine;
using System.Collections;
public class Box : Base
{
    [RPC]
    void SetScopeTrue(NetworkMessageInfo info)
    {
        Trace.LogError(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>Set SCope By SErveR?" + (info.sender == Network.player));
        foreach (NetworkPlayer p in Network.connections)
            info.networkView.SetScope(p, true);
        SetOwner(info.networkView.owner);
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
        
        CallLast(Group.Rig, "SetOwner", np);
        OwnerID = np;        
    }



    protected override void OnPlayerConnected(NetworkPlayer player)
    {
        foreach (NetworkView nw in this.GetComponents<NetworkView>())
            if (!nw.isMine)
                nw.SetScope(player, false);
    }

    
} 

































