using UnityEngine;
using System.Collections;
using System;
using System.Runtime.CompilerServices;
using doru;

public enum Weapon : int{ balls =1 , AntiGrawitaty = 2};
public class PL : Base
{
    protected override void Awake()
    {
        enabled = false;
    }
    public float angularvel = 600;
    public float force = 400;
    public float antigrvForce = 300;
    public int minimumy = -8;
    public NetworkPlayer killedyby;
    public static PL _LocalPlayer;
    public bool isdead;
    public double m_InterpolationBackTime = 0.1;
    public double m_ExtrapolationLimit = 0.5;
    public int score;
    State[] m_BufferedState = new State[20];
    int m_TimestampCount;
    protected TextMesh txt { get { return transform.Find("txtholder/Nick").GetComponent<TextMesh>(); } }
    public Quaternion rotation { get { return gun.q; } set { gun.q = value; } }
    
    Cam _cam { get { return Cam._This; } }
    public TimerA _TimerA { get { return SpawnPrefab._This._TimerA; } }
    SpawnPrefab _SpawnPrefab { get { return SpawnPrefab._This; } }
    int life;
    public int Life
    {
        get { return life; }
        set
        {
            if (isMine)
            {
                LifeChanged(value);
                life = value;
            }
        }
    }
    blood blood { get { return blood._This; } }
    public bool isMine { get { return networkView.isMine; } }
    protected override void Start()
    {        
 
    }
    
    public override void OnSetID()
    {
        enabled = true;
        if (isMine)
        {
            txt.renderer.enabled = false;
            _LocalPlayer = this;
            Spawn();
        }
        else
        {
            name += "remote";
        }

        base.OnSetID();
    }
    
    [RPC]
    public void Spawn()
    {

        if (isMine)
        {
            CallLast(Group.Spawn, "Spawn");
            //networkView.RPC("Spawn", RPCMode.Others); 
            _LocalPlayer.isdead = false;
        }
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        gun.SetWeaponN((int)Weapon.balls);
        gun.SetMoveN((int)MoveType.Walk);
        ShowDeath(false);
        life = 100;
        transform.position = _SpawnPrefab.SpawnPoint();
        enabled = true;

    }
    Gun gun { get { return (Gun)this.GetComponentInChildren(typeof(Gun)); } }

    public Weapon weapon { get { return gun.weapon; } }
    protected override void Update()
    {
        if (isMine)
        {
            if (Input.GetKeyDown(KeyCode.K)) this.Life = -1;                        
            
        }
        else RemoteUpdate();

    }
    protected override void FixedUpdate()
    {
        if (isMine)
        {
            PL pl = this;
            Cam cam = Cam._This;
            if (transform.position.y < minimumy) this.Life = -1;
            MoveMent();
            
        }
    }

    void OnCollisionEnter(Collision collisionInfo)
    {
        if (isMine)
            foreach (ContactPoint a in collisionInfo.contacts)
                if (a.otherCollider.tag == "Rig" && a.otherCollider.rigidbody.velocity.magnitude > 20)
                {
                    //killedyby = a.otherCollider.GetComponent<Base>().OwnerID.Value;
                    Life -= (int)a.otherCollider.rigidbody.velocity.magnitude;
                }
    }

    public float speed = 10.0f;
    public float gravity = 10.0f;
    public float maxVelocityChange = 10.0f;
    public bool canJump = true;
    public float jumpHeight = 2.0f;



    private Vector3 Clamp(Vector3 velocityChange)
    {
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = Mathf.Clamp(velocityChange.y, -maxVelocityChange, maxVelocityChange);
        return velocityChange;
    }
    //void OnCollisionStay()
    //{
    //    grounded = true;
    //}
    private void FpsMovement()
    {
        Vector3 targetVelocity = Vector3.zero;
        targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        targetVelocity = _cam.transform.TransformDirection(targetVelocity);
        targetVelocity *= force * Time.deltaTime;
        Vector3 vcur = rigidbody.velocity;
        vcur.y = 0;
        Vector3 velocityChange = (targetVelocity - vcur);
        this.rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
    }
    

    private void MoveMent()
    {
        Vector3 moveDirection = Vector3.zero;

        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveDirection = _cam.transform.TransformDirection(moveDirection);
            
        if (gun.Move == MoveType.Fly)
        {
            Fly(moveDirection);
        }
        if (gun.Move == MoveType.Walk)
        {
            FpsMovement();
        }
        if (gun.Move == MoveType.Norm)
        {
            
            moveDirection.y = 0;
            moveDirection.Normalize();

            this.rigidbody.AddTorque(new Vector3(moveDirection.z, 0, -moveDirection.x) * Time.deltaTime * angularvel);
            //if (this.rigidbody.velocity.sqrMagnitude < maxvel || (this.rigidbody.velocity - moveDirection).magnitude > this.rigidbody.velocity.magnitude)
            this.rigidbody.AddForce(moveDirection * Time.deltaTime * force);
        }
    }

    private void Fly(Vector3 moveDirection)
    {
        
        if (Input.GetKey(KeyCode.LeftControl))
            moveDirection.y -= 1;
        if (Input.GetKey(KeyCode.Space))
            moveDirection.y += 1;
        moveDirection.Normalize();                
        this.rigidbody.AddForce(moveDirection * Time.deltaTime * antigrvForce);
        this.rigidbody.velocity = Clamp(this.rigidbody.velocity);        
    }

    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        
        // Send data to server
        if (stream.isWriting)
        {
            Vector3 pos = rigidbody.position;
            //Quaternion rot = rigidbody.rotation;
            Quaternion rot = rotation;
            Vector3 velocity = rigidbody.velocity;
            Vector3 angularVelocity = rigidbody.angularVelocity;

            stream.Serialize(ref pos);
            stream.Serialize(ref velocity);
            stream.Serialize(ref rot);
            stream.Serialize(ref angularVelocity);
            //stream.Serialize(ref rot2);
        }
        // Read data from remote client
        else
        {
            Vector3 pos = Vector3.zero;
            Vector3 velocity = Vector3.zero;
            Quaternion rot = Quaternion.identity;
            Vector3 angularVelocity = Vector3.zero;
            stream.Serialize(ref pos);
            stream.Serialize(ref velocity);
            stream.Serialize(ref rot);
            stream.Serialize(ref angularVelocity);

            // Shift the buffer sideways, deleting state 20
            for (int i = m_BufferedState.Length - 1; i >= 1; i--)
            {
                m_BufferedState[i] = m_BufferedState[i - 1];
            }

            // Record current state in slot 0
            State state;
            state.timestamp = info.timestamp;
            state.pos = pos;
            state.velocity = velocity;
            state.rot = rot;
            state.angularVelocity = angularVelocity;
            m_BufferedState[0] = state;

            // Update used slot count, however never exceed the buffer size
            // Slots aren't actually freed so this just makes sure the buffer is
            // filled up and that uninitalized slots aren't used.
            m_TimestampCount = Mathf.Min(m_TimestampCount + 1, m_BufferedState.Length);

            // Check if states are in order, if it is inconsistent you could reshuffel or 
            // drop the out-of-order state. Nothing is done here
            for (int i = 0; i < m_TimestampCount - 1; i++)
            {
                if (m_BufferedState[i].timestamp < m_BufferedState[i + 1].timestamp)
                    Debug.Log("State inconsistent");
            }
        }
    }

    protected void RemoteUpdate()
    {
        if (txt.text.Length == 0 && PLView.pls.ContainsKey(OwnerID.Value))
            txt.text = PLView.pls[OwnerID.Value].Nick;
    

        // This is the target playback time of the rigid body
        double interpolationTime = Network.time - m_InterpolationBackTime;

        // Use interpolation if the target playback time is present in the buffer
        if (m_BufferedState[0].timestamp > interpolationTime)
        {
            // Go through buffer and find correct state to play back
            for (int i = 0; i < m_TimestampCount; i++)
            {
                if (m_BufferedState[i].timestamp <= interpolationTime || i == m_TimestampCount - 1)
                {
                    // The state one slot newer (<100ms) than the best playback state
                    State rhs = m_BufferedState[Mathf.Max(i - 1, 0)];
                    // The best playback state (closest to 100 ms old (default time))
                    State lhs = m_BufferedState[i];

                    // Use the time between the two slots to determine if interpolation is necessary
                    double length = rhs.timestamp - lhs.timestamp;
                    float t = 0.0F;
                    // As the time difference gets closer to 100 ms t gets closer to 1 in 
                    // which case rhs is only used
                    // Example:
                    // Time is 10.000, so sampleTime is 9.900 
                    // lhs.time is 9.910 rhs.time is 9.980 length is 0.070
                    // t is 9.900 - 9.910 / 0.070 = 0.14. So it uses 14% of rhs, 86% of lhs
                    if (length > 0.0001)
                        t = (float)((interpolationTime - lhs.timestamp) / length);

                    // if t=0 => lhs is used directly
                    transform.localPosition = Vector3.Lerp(lhs.pos, rhs.pos, t);
                    rotation = Quaternion.Slerp(lhs.rot, rhs.rot, t);
                    return;
                }
            }
        }
        // Use extrapolation
        else
        {
            State latest = m_BufferedState[0];

            float extrapolationLength = (float)(interpolationTime - latest.timestamp);
            // Don't extrapolation for more than 500 ms, you would need to do that carefully
            if (extrapolationLength < m_ExtrapolationLimit)
            {
                float axisLength = extrapolationLength * latest.angularVelocity.magnitude * Mathf.Rad2Deg;
                Quaternion angularRotation = Quaternion.AngleAxis(axisLength, latest.angularVelocity);

                rigidbody.position = latest.pos + latest.velocity * extrapolationLength;
                rotation = angularRotation * latest.rot;
                rigidbody.velocity = latest.velocity;
                rigidbody.angularVelocity = latest.angularVelocity;
            }
        }
    }
    
    
    
    [RPC]
    protected virtual void Die()
    {
        if (isMine)
        {
            
            //networkView.group = (int)Group.Spawn;
            //networkView.RPC("Die", RPCMode.OthersBuffered);
        }
        isdead = true;
        enabled = false;
        _TimerA.AddMethod(1000, Spawn);
        ShowDeath(true);
        if (isMine)
        {
            PLView plv = PLView.pls[killedyby];
            plv.networkView.RPC("AddPoint", RPCMode.All, killedyby == Network.player ? -1 : 1);
        }
    }
    public void ShowDeath(bool a)
    {
        transform.Find("dead").renderer.enabled = a;
    }

    
    
    private void LifeChanged(int nwlife)
    {
        blood.Hit(Mathf.Abs(nwlife - life));
        if (nwlife < 0)
            Die();          
        
    }


    
    
}
internal struct State
{
    internal double timestamp;
    internal Vector3 pos;
    internal Vector3 velocity;
    internal Quaternion rot;
    internal Vector3 angularVelocity;
}


//private void Movement2()
//{

//    if (grounded)
//    {
//        // Calculate how fast we should be moving
//        Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
//        targetVelocity = transform.TransformDirection(targetVelocity);
//        targetVelocity *= speed;

//        // Apply a force that attempts to reach our target velocity
//        Vector3 velocity = rigidbody.velocity;
//        Vector3 velocityChange = (targetVelocity - velocity);
//        velocityChange = Clamp(velocityChange);
//        rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);

//        // Jump
//        if (canJump && Input.GetButton("Jump"))
//        {
//            rigidbody.velocity = new Vector3(velocity.x, Mathf.Sqrt(2 * jumpHeight * gravity), velocity.z);
//        }
//    }

//    // We apply gravity manually for more tuning control
//    rigidbody.AddForce(new Vector3(0, -gravity * rigidbody.mass, 0));

//    grounded = false;


//}