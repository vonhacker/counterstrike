using UnityEngine;
using System.Collections;
public class Box : Base
{

    Vector2 spawn;
    protected override void Start()
    {
        spawn = this.transform.position;
    }
    
    protected override void Update()
    {
        if (!GameObject.Find("Cube").collider.bounds.Contains(this.transform.position))
        {
            this.transform.rigidbody.velocity = Vector3.zero;
            this.transform.position = spawn;
        }
    }
    [RPC]
    void SetOwner(NetworkPlayer owner, NetworkMessageInfo a)
    {
        foreach (NetworkView b in this.GetComponents<NetworkView>())
            b.observed = null;
        a.networkView.observed = this.rigidbody;
        OwnerID = owner;
    }

    
} 

















