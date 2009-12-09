using UnityEngine;
using System.Collections;
using System.Threading;
using System;

public enum MoveType:int { Fly, Norm, Walk }
public enum Group:int
{
    deflt = 0,
    Spawn = 1,
    PLView= 2,
    Patron =3,
    SetWeapon = 4,
    MouseDown = 5,
    Player = 6,
    Rig = 7,
    Fly = 8
}
public static class sw
{
    static System.Diagnostics.Stopwatch sp= new System.Diagnostics.Stopwatch();
    static sw()
    {
        sp.Start();
    }
    static  long l;
    public static string Check()
    {
        long a = l;
        l = sp.ElapsedMilliseconds;
        return "ticks:" + (sp.ElapsedMilliseconds - a);
    }
}
public class Gun : Base {


    // Use this for initialization
    protected override void Start()
    {        
        if (IsMine)
            foreach (GameObject a in GameObject.FindGameObjectsWithTag("Rig"))
            {
                a.rigidbody.angularDrag = grawangdrag;
                a.rigidbody.drag = grawdrag;
                a.rigidbody.mass = grawmass;
            }
    }
    public LayerMask lm;
    //public Transform _Patron;

    Cam cam { get { return Cam._This; } }
    Transform cur2 { get { return transform.Find("cursor2").transform; } }
    public Transform _Patron;
    public int gravdist = 20;
    public int grawforce = 20;
    public float grawangdrag = 15f;
    public float grawdrag = .5f;
    public float ShootPower = 5000f;
    public float grawmass = .5f;
    public ForceMode _ForceMode = ForceMode.Acceleration;
    
    protected override void Awake()
    {
        enabled = false;
    }
    public override void OnSetID()
    {
        enabled = true;
    }
    public bool mousedown1;
    public bool MouseDown1
    {
        get { return mousedown1; }
        set
        {
            if (mousedown1 != value) MouseDown1Changed(value);
        }
    }
    [RPC]
    void MouseDown1Changed(bool value)
    {

        CallLast(Group.MouseDown, "MouseDown1Changed", value);
        mousedown1 = value;
    }
    [RPC]
    void SetWeapon(int id)
    {
        CallLast(Group.SetWeapon, "SetWeapon", id);
        SetWeaponN(id);
    }

    public void SetWeaponN(int id)
    {
        weapon = (Weapon)id;
        transform.Find("model").renderer.enabled = false;
        if (weapon == Weapon.balls)
            transform.Find("model").renderer.enabled = true;
    }
    [RPC]
    void ReleaseGravitaty()
    {
        
        foreach (GameObject a in GameObject.FindGameObjectsWithTag("Rig"))
            if (Vector3.Distance(a.transform.position, cur2.position) < gravdist)
                a.rigidbody.AddForce(transform.TransformDirection(Vector3.forward) * ShootPower);
    }
    [RPC]
    void Shoot(Vector3 pos, Quaternion q)
    {
        Call("Shoot", pos, q);
        Trace.Log("shoot " + OwnerID);
        (Instantiate(_Patron, pos, q) as Transform).GetComponent<Patron>().OwnerID = OwnerID.Value;        
    }

    public MoveType Move = MoveType.Norm;
    

    [RPC]
    void SetMove(int v)
    {
        CallLast(Group.Fly, "SetMove", v);
        SetMoveN(v);
        Move = (MoveType)v;
    }

    public void SetMoveN(int v)
    {
        if (MoveType.Fly == (MoveType)v)
        {
            pl.rigidbody.freezeRotation = true; 
            pl.rigidbody.useGravity = false;
        }
        else
        {
            pl.rigidbody.freezeRotation = false;
            pl.rigidbody.useGravity = true;
        }
    }
    
    protected override void Update()
    {
        if (!IsMine) transform.Find("model").renderer.enabled = false;
        if (IsMine) q = Cam._This.transform.rotation;
        this.transform.rotation = q;


        if (MouseDown1 && weapon == Weapon.AntiGrawitaty)
        {
            foreach(GameObject a in GameObject.FindGameObjectsWithTag("Rig"))
                a.rigidbody.AddExplosionForce(-grawforce, cur2.position, gravdist);
        }        

        if (IsMine)
            LocalUpdate();
    }
    
    

    private void LocalUpdate()
    {
        
        MouseDown1 = Input.GetMouseButton(1);
        //UpdateGravityGun();
        if (!Screen.lockCursor) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetWeapon((int)Weapon.balls);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            SetWeapon((int)Weapon.AntiGrawitaty);

        if (Input.GetKeyDown(KeyCode.F))
            SetMove((int)MoveType.Fly);
        if (Input.GetKeyDown(KeyCode.G))
            SetMove((int)MoveType.Norm);
        if (Input.GetKeyDown(KeyCode.H))
            SetMove((int)MoveType.Walk);
        
        if (Input.GetMouseButtonDown(0) && weapon == Weapon.balls)
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));//new Ray(cam.transform.position, cam.transform.TransformDirection(Vector3.forward));  
            ray.origin = ray.GetPoint(10);
            RaycastHit h;
            if (Physics.Raycast(ray, out h, float.MaxValue, lm.value))
                transform.LookAt(h.point);
            else
                transform.rotation = cam.transform.rotation;
            
            Shoot(transform.position, transform.rotation);
        }


        if (Input.GetMouseButtonDown(0) && weapon == Weapon.AntiGrawitaty)
            ReleaseGravitaty();

    }

    private void UpdateGravityGun()
    {
        GameObject[] cts = GameObject.FindGameObjectsWithTag("Rig");
        for (int i = 0; i < cts.Length; i++)
        {
            GameObject a = cts[i];
            if (Vector3.Distance(a.transform.position, cur2.position) < gravdist)
            {
                if (MouseDown1 && weapon == Weapon.AntiGrawitaty && (a.networkView.viewID == NetworkViewID.unassigned || !a.networkView.isMine))
                    AssignID(i, Network.AllocateViewID());
            }
            
        }
    }
    
    [RPC]
    public void AssignID(int i, NetworkViewID id)
    {
        
        RPC(Group.Rig, RPCMode.OthersBuffered, "AssignID", i, id);
        GameObject g = GameObject.FindGameObjectsWithTag("Rig")[i];
        Rig rig = g.GetComponent<Rig>();        
        rig.OwnerID = OwnerID;        
        g.networkView.viewID = id;
        Trace.Log("Assinged network id" + g.networkView.viewID);
    }

    
    PL pl { get { return this.transform.parent.GetComponent<PL>(); } }
    public Weapon weapon = Weapon.balls;
    public Quaternion q = Quaternion.identity;
    
}
//else
//    if (a.networkView.isMine && a.networkView.viewID != NetworkViewID.unassigned) AssignID(i, NetworkViewID.unassigned);
//this.transform.Rotate(Vector3.up);
//Call("ReleaseGravitaty");
//networkView.RPC(ReleaseGravitaty, RPCMode.OthersBuffered);
        //if ((Weapon)id == Weapon.AntiGrawitaty)
        //{
        //    pl.rigidbody.freezeRotation = true;
        //    pl.rigidbody.useGravity = false;
        //}
        //else
        //{
        //    pl.rigidbody.freezeRotation = false;
        //    pl.rigidbody.useGravity = true;
        //}