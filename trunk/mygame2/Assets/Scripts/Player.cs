using UnityEngine;
using System.Collections;
using doru;

public class Player : Base {

    Cam _cam { get { return Find<Cam>(); } }
    Blood blood { get { return Find<Blood>(); } }
    Spawn spawn { get { return Find<Spawn>(); } }
    public bool isdead { get { return !enabled; } }
    public float force = 400;
    public float angularvel = 600;
    public int Life;
    public int Nick;
    public int score;
    

    protected override void Start()
    {
        
        if (Network.isServer && !isMine)
            foreach (Player p in Component.FindObjectsOfType(typeof(Player)))
                p.SetScore(p.score);

        if (isMine)
        {
            SetID(Network.player);
            foreach (GameObject a in GameObject.FindGameObjectsWithTag("Box"))
                AssignID(int.Parse(a.name), Network.AllocateViewID());
        }
    }
    
    [RPC]
    public void SetID(NetworkPlayer player)
    {
        
        CallBuffered(Group.Player, "SetID", player);
        foreach (Base a in GetComponentsInChildren(typeof(Base)))
        {
            a.OwnerID = player;
            a.OnSetID();
        }
    }

    [RPC]
    public void SetLife(int NwLife)
    {
        CallLast(Group.Life, "SetLife", NwLife);
        if (isMine)
            blood.Hit(Mathf.Abs(NwLife - Life));
                
        if (NwLife < 0)
            Die();
        Life = NwLife;

    }

    TimerA _TimerA { get { return Find<FpsCounter>().timer; } }
    [RPC]
    public void Spawn()
    {
        Call("Spawn");
        Show(true);
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        Life = 100;
        transform.position = SpawnPoint();
    }

    private void Show(bool value)
    {
        enabled = value;
        renderer.enabled = value;
    }
    

    protected virtual void Die()
    {
        
        if (isMine)
        {
            _TimerA.AddMethod(2000, Spawn);            
        }

        Show(false);
        
        if (isMine)
        {
            foreach(Player p in GameObject.FindObjectsOfType(typeof(Player)))
                if(p.OwnerID == killedyby)
                     SetScore(score + (killedyby == Network.player ? -1 : 1));            
        }
    }
    
    [RPC]
    public void SetScore(int i)
    {
        Call("SetScore", i);
        score = i;
    }
            
    public Vector3 SpawnPoint()
    {
        return spawn.transform.GetChild(Random.Range(0, transform.childCount)).transform.position;
    }

    
    public NetworkPlayer killedyby;
    [RPC]
    public void AssignID(int i, NetworkViewID id)
    {
        CallBuffered(Group.Rig, "AssignID",i, id);
        Trace.Log("Assign index:" + i + " id:" + id + " isMine:" + isMine);
        GameObject g = GameObject.Find(i.ToString());        
        NetworkView nw = g.AddComponent<NetworkView>();
        nw.group = (int)Group.Rig;
        nw.observed = g.rigidbody;
        nw.stateSynchronization = NetworkStateSynchronization.ReliableDeltaCompressed;
        nw.viewID = id;
    }

    
    public override void OnSetID()
    {
        if (!isMine) name += "Remote";        
    }
    protected override void FixedUpdate()
    {
        if(isMine) 
            LocalFixedUpdate();
        
    }

    protected override void OnCollisionEnter(Collision collisionInfo)
    {
        if (isMine)
            foreach (ContactPoint a in collisionInfo.contacts)
                if (a.otherCollider.tag == "Box" && a.otherCollider.rigidbody.velocity.magnitude > 20)
                {
                    killedyby = a.otherCollider.GetComponent<Base>().OwnerID.Value;
                    SetLife(Life - (int)a.otherCollider.rigidbody.velocity.magnitude);
                }

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
