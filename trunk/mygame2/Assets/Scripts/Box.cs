using UnityEngine;
using System.Collections;
public class Box : Base
{

    Vector3 spawn;
    Quaternion rotation;
    protected override void Start()
    {
        spawn = this.transform.position;
        rotation = this.transform.rotation;
    }
    
    protected override void Update()
    {
        if (!GameObject.Find("Cube").collider.bounds.Contains(this.transform.position))
        {
            Reset();
        }
    }

    public void Reset()
    {
        this.transform.rigidbody.velocity = Vector3.zero;
        this.transform.rigidbody.angularVelocity = Vector3.zero;
        this.transform.position = spawn;
        this.transform.rotation = rotation;
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

















