using UnityEngine;
using System.Collections;
public enum Group { PlView, Player, Default,Rig,Life,Spawn }    
public class Spawn : Base
{
    public Transform _Player;

    protected override void OnNetworkLoadedLevel()
    {
        Network.Instantiate(_Player, Vector3.zero, Quaternion.identity, (int)Group.Player);
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
    
}
  