using UnityEngine;
using System.Collections;
using doru;
public enum Movement : int { Fly, Move }
public class Player : Base {
    public float flyForce = 300;
    public float maxVelocityChange = 10.0f;
    Cam _cam { get { return Find<Cam>(); } }
    Blood blood { get { return Find<Blood>(); } }
    public static Spawn spawn { get { return Find<Spawn>(); } }
    ConnectionGUI connectionGui { get { return Find<ConnectionGUI>(); } }
    TimerA _TimerA { get { return Find<FpsCounter>().timer; } }
    GameObject boxes { get { return GameObject.Find("box"); } }
    public bool isdead { get { return !enabled; } }
    public float force = 400;
    public float angularvel = 600;
    public int Life;
    public string Nick;
    public int score;
    

    protected override void Start()
    { 
        Trace.Log(">>>>>>>>>>>>>>>>>>>player Created" + networkView.owner);
        if (isMine)            
        {
            RPCSetNick(connectionGui.Nick);
            RPCSetID(Network.player);

            Object[] gs = GameObject.FindObjectsOfType(typeof(Box));
            for (int i = 0; i <  gs.Length; i++)
                RPCAssignID(i, Network.AllocateViewID());
                
            RPCSpawn();
        }
        
    }
    [RPC]
    public void RPCAssignID(int i, NetworkViewID id)
    {
        Object[] gs = GameObject.FindObjectsOfType(typeof(Box));
        CallBuffered(Group.RPCAssignID, "RPCAssignID", i, id);
        GameObject g = ((Box)gs[i]).gameObject;        
        NetworkView nw = g.AddComponent<NetworkView>();
        nw.group = (int)Group.RPCAssignID;
        nw.observed = null;
        nw.stateSynchronization = NetworkStateSynchronization.ReliableDeltaCompressed;
        nw.viewID = id;
    }

    public override void OnSetID()
    {
        if (isMine)
            name = "LocalPlayer";
        else
            name = "RemotePlayer" + OwnerID;
    }
    protected override void FixedUpdate()
    {
        if (isMine)
            LocalFixedUpdate();

    }
    protected override void Update()
    {
        if (isMine)
            if (Input.GetKeyDown(KeyCode.F))
                RPCSetMovement((int)(movement == Movement.Fly ? Movement.Move : Movement.Fly));
    }
    private void LocalFixedUpdate()
    {
        

        if (movement == Movement.Move)
            LocalMove();
        else if (movement == Movement.Fly)
            LocalFly();
    }
    private void LocalMove()
    {
        Vector3 moveDirection = Vector3.zero;
        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveDirection = _cam.transform.TransformDirection(moveDirection);
        moveDirection.y = 0;
        moveDirection.Normalize();
        this.rigidbody.AddTorque(new Vector3(moveDirection.z, 0, -moveDirection.x) * Time.deltaTime * angularvel);
        this.rigidbody.AddForce(moveDirection * Time.deltaTime * force);
    }
    private void LocalFly()
    {
        Vector3 moveDirection = Vector3.zero;
        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveDirection = _cam.transform.TransformDirection(moveDirection);
        if (Input.GetKey(KeyCode.LeftControl))
            moveDirection.y -= 1;
        if (Input.GetKey(KeyCode.Space))
            moveDirection.y += 1;
        moveDirection.Normalize();
        this.rigidbody.AddForce(moveDirection * Time.deltaTime * flyForce);
        this.rigidbody.velocity = Clamp(this.rigidbody.velocity,maxVelocityChange);
    }
    
    public static Vector3 SpawnPoint()
    {
        return spawn.transform.GetChild(Random.Range(0, spawn.transform.childCount)).transform.position;
    }

    

    [RPC]
    void RPCSetNick(string nick)
    {
        CallLast(Group.Nick, "RPCSetNick", nick);
        Nick = nick;
    }
    [RPC]
    public void RPCSetID(NetworkPlayer player)
    {

        CallBuffered(Group.RPCSetID, "RPCSetID", player);
        foreach (Base a in GetComponentsInChildren(typeof(Base)))
        {
            a.OwnerID = player;
            a.OnSetID();
        }
    }

    [RPC]
    public void RPCSetLife(int NwLife)
    {
        CallLast(Group.Life, "RPCSetLife", NwLife);
        if (isMine)
        {                        
            blood.Hit(Mathf.Abs(NwLife - Life));
        }       
        if (NwLife < 0)
            RPCDie();
        Life = NwLife;

    }

    

    [RPC]
    public void RPCSpawn()
    {
        CallLast(Group.Spawn,"RPCSpawn");
        Show(true);
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        Life = 100;
        transform.position = SpawnPoint();
    }
    [RPC]
    public void RPCSetScore(int i)
    {        
        score = i;
    }
    

    private void Show(bool value)
    {
        enabled = value;
        renderer.enabled = value;
    }

    public void RPCDie()
    {
        
        if (isMine)
        {
            _TimerA.AddMethod(2000, RPCSpawn);
            foreach (Player p in GameObject.FindObjectsOfType(typeof(Player)))
                if (p.OwnerID == killedyby)
                {                    
                    if (p.isMine)
                        networkView.RPC("RPCSetScore", RPCMode.All, score - 1);                        
                    else
                        p.networkView.RPC("RPCSetScore", RPCMode.All, p.score + 1);                        
                }

        }

        Show(false);                
    }
    
    public NetworkPlayer killedyby;
    
    protected override void OnCollisionEnter(Collision collisionInfo)
    {
        if (isMine)
            foreach (ContactPoint a in collisionInfo.contacts)
                if (a.otherCollider.GetComponent<Box>() != null && a.otherCollider.rigidbody.velocity.magnitude > 10 && enabled)
                {
                    killedyby = a.otherCollider.GetComponent<Base>().OwnerID.Value;
                    RPCSetLife(Life - (int)a.otherCollider.rigidbody.velocity.sqrMagnitude);
                }

    }
    
    
    public static Vector3 Clamp(Vector3 velocityChange,float maxVelocityChange)
    {
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = Mathf.Clamp(velocityChange.y, -maxVelocityChange, maxVelocityChange);
        return velocityChange;
    }

    public Movement movement = Movement.Move;
    [RPC]
    void RPCSetMovement(int mode)
    {
        
        CallLast(Group.SetMovement, "RPCSetMovement", mode);
        movement = (Movement)mode;
        this.rigidbody.useGravity = (movement == Movement.Fly ? false : true);
    }
    
}
