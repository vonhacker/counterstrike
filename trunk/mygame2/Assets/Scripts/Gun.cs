using UnityEngine;
using System.Collections;
public class Gun : Base
{
    public enum Weapon { AntiGrawitaty }
    Weapon weapon;
    Quaternion q;
    public int gravdist = 20;
    public float ShootPower = 5000f;
    public int grawforce = 20;
    public bool MouseDown1;
    void LocalSetMouseDown(bool b)
    {
        if (MouseDown1 == b) return;
        MouseDown1 = b;
        
    }
    Transform cur2 { get { return transform.Find("cursor2").transform; } }

    protected override void Update()
    {
        if (isMine) q = Find<Cam>().transform.rotation;
        this.transform.rotation = q;
        if (isMine)
            LocalUpdate();

    }

    private void LocalUpdate()
    {
        
        LocalSetMouseDown(Input.GetMouseButton(1));
     
        if (Screen.lockCursor && Input.GetMouseButtonDown(0)) 
            LocalReleaseGravitaty();        
        LocalUpdateOwners();
        LocalUpdateGravityGun();
    }

    private void LocalUpdateGravityGun()
    {
        if (MouseDown1 && weapon == Weapon.AntiGrawitaty)
            foreach (GameObject a in GameObject.FindGameObjectsWithTag("Box"))
            {
                Box box = a.GetComponent<Box>();
                if (box.OwnerID != null && box.OwnerID == Network.player)
                    UpdateBox(box);
            }
    }

    private void UpdateBox(Box box)
    {        
        //box.rigidbody.AddExplosionForce(-grawforce, cur2.position, gravdist, 0, ForceMode.Force); 
        //box.rigidbody.velocity *= .9f;
        
        Vector3 v = (cur2.position - box.transform.position);
        if (v.magnitude < gravdist)
        {
            box.rigidbody.velocity += (v.normalized * grawforce) / 50; //.AddExplosionForce(-grawforce, cur2.position, gravdist, 0, ForceMode.Force); 
            box.rigidbody.velocity *= .9f;
            if (v.magnitude < .3) box.transform.position = cur2.position;
        }
    }
    private void LocalUpdateOwners()
    {

        foreach (GameObject a in GameObject.FindGameObjectsWithTag("Box"))
            if (Input.GetMouseButtonDown(1) && Vector3.Distance(a.transform.position, cur2.position) < gravdist)
                foreach (NetworkView b in a.GetComponents<NetworkView>())
                    if (b.isMine)
                    {                                   
                        b.RPC("SetOwner", RPCMode.All, Network.player);
                    }
    }
    
    void LocalReleaseGravitaty()
    {
        foreach (GameObject a in GameObject.FindGameObjectsWithTag("Box"))
            if (Vector3.Distance(a.transform.position, cur2.position) < gravdist)
                a.rigidbody.AddForce(transform.TransformDirection(Vector3.forward) * ShootPower);
    }

  


}




