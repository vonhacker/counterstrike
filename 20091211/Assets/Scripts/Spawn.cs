using UnityEngine;
using System.Collections;
public enum Group { PlView, Player, Default,Rig,SetOwner }    
public class Spawn : Base
{
    public Transform _Player;

    protected override void OnNetworkLoadedLevel()
    {
        NetworkInstantiate(_Player, Vector3.zero, Quaternion.identity, Group.Player);        
    }
    protected override void Start()
    {

        
        
    }

    protected override void OnPlayerDisconnected(NetworkPlayer player)
    {

        foreach (GameObject a in GameObject.FindGameObjectsWithTag("Box"))
            foreach (NetworkView v in a.GetComponents<NetworkView>())
                if (v.owner == player) Destroy(v);
        Network.DestroyPlayerObjects(player);
        Network.RemoveRPCs(player);        
    }
    
    private void Destroy(NetworkView nw)
    {
        nw.viewID = NetworkViewID.unassigned;
        nw.enabled = false;
        Component.Destroy(nw);

    }

    public void NetworkInstantiate(Object prefab, Vector3 position, Quaternion rotation, Group group)
    {
        Transform t = (Transform)Network.Instantiate(prefab, position, rotation, (int)group);
        t.networkView.group = (int)group;
        t.networkView.RPC("SetID", RPCMode.AllBuffered, Network.player);
    }
    
}
  