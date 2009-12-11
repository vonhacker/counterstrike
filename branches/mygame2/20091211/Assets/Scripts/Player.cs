using UnityEngine;
using System.Collections;

public class Player : Base {

    Cam _cam { get { return Find<Cam>(); } }
    public bool isdead;    
    public float force = 400;
    public float angularvel = 600;
    protected override void Start()
    {
        if (IsMine)
            foreach (GameObject a in GameObject.FindGameObjectsWithTag("Box"))
                AssignID(int.Parse(a.name), Network.AllocateViewID());
        
    } 
 
    [RPC]
    public void AssignID(int i, NetworkViewID id)
    {
        CallBuffered(Group.Rig, "AssignID",i, id);
        GameObject g = GameObject.Find(i.ToString());        
        NetworkView nw = g.AddComponent<NetworkView>();
        nw.group = (int)Group.Rig;
        nw.observed = g.rigidbody;
        nw.stateSynchronization = NetworkStateSynchronization.ReliableDeltaCompressed;
        nw.viewID = id;        
    }

    protected override void Awake()
    { 
        enabled = false;
    }
    public override void OnSetID()
    {
        if (!IsMine) name += "Remote";
        enabled = true;
    }
    protected override void FixedUpdate()
    {
        if(IsMine) 
            LocalFixedUpdate();
        
    }

    private void LocalFixedUpdate()
    {
        
        Vector3 moveDirection = Vector3.zero;
        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveDirection = _cam.transform.TransformDirection(moveDirection);
        moveDirection.y = 0;
        moveDirection.Normalize();
        this.rigidbody.AddTorque(new Vector3(moveDirection.z, 0, -moveDirection.x) * Time.deltaTime * angularvel);
        this.rigidbody.AddForce(moveDirection * Time.deltaTime * force);
    }
}
