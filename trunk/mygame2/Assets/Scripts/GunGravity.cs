using UnityEngine;
using System.Collections;
public class GunGravity : GunBase
{
    public enum Weapon { AntiGrawitaty, RocketLauncher }    
    public static Weapon weapon;
    
    public int gravdist = 20;
    public int shootdist = 10;
    public float ShootPower = 5000f;
    public int grawforce = 20;
    
    Transform cur2 { get { return transform.Find("cursor2").transform; } }
    public float dt { get { return Time.deltaTime * 10; } }
    
    
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }
    protected override void Update()
    {
        base.Update();
        if (isMine)
            LocalUpdate();
    }
    
    
    protected override void LocalUpdate()
    {
        if (Screen.lockCursor && bullets > 0)
        {
            if (Screen.lockCursor && Input.GetMouseButtonDown(0))
                LocalReleaseGravitaty();
            LocalUpdateOwners();
            if (Input.GetMouseButton(1) && weapon == Weapon.AntiGrawitaty)
                LocalUpdateGravityGun();
        }
    }
    
    

    
    
    private void LocalUpdateGravityGun()
    {
        foreach (Box box in GameObject.FindObjectsOfType(typeof(Box)))
        {
            if (box.OwnerID != null && box.OwnerID == Network.player)
                UpdateBox(box);
        }
        bullets -= dt * 1.2f;
    }
    void LocalReleaseGravitaty()
    {
        foreach (Box a in GameObject.FindObjectsOfType(typeof(Box)))
            if (Vector3.Distance(a.transform.position, cur2.position) < shootdist)
            {
                if (!a.rigidbody.IsSleeping())
                    a.rigidbody.velocity += (transform.TransformDirection(Vector3.forward) * ShootPower);
            }
        bullets -= 50;
    }
    private void UpdateBox(Box box)
    {        
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
        foreach (Box a in GameObject.FindObjectsOfType(typeof(Box)))
            if (Input.GetMouseButtonDown(1) && Vector3.Distance(a.transform.position, cur2.position) < gravdist)
                foreach (NetworkView b in a.GetComponents<NetworkView>())
                    if (b.isMine)
                        b.RPC("SetOwner", RPCMode.All, Network.player);
    }
    
    

  


}





//box.rigidbody.AddExplosionForce(-grawforce, cur2.position, gravdist, 0, ForceMode.Force); 
//box.rigidbody.velocity *= .9f;