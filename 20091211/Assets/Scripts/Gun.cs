using UnityEngine;
using System.Collections;
public class Gun : Base
{
    public enum Weapon { AntiGrawitaty }
    Weapon weapon;
    Quaternion q;
    public int gravdist = 20;
    public int grawforce = 20;
    public bool MouseDown1;
    void SetMouseDown(bool b)
    {
        if (MouseDown1 == b) return;
        MouseDown1 = b;

    }
    Transform cur2 { get { return transform.Find("cursor2").transform; } }

    protected override void Update()
    {
        if (IsMine) q = Find<Cam>().transform.rotation;
        this.transform.rotation = q;
        if (IsMine)
            LocalUpdate();

    }

    private void LocalUpdate()
    {
        SetMouseDown(Input.GetMouseButton(1));
        UpdateOwners();
        if (MouseDown1 && weapon == Weapon.AntiGrawitaty)
            foreach (GameObject a in GameObject.FindGameObjectsWithTag("Box"))
            {
                Box box = a.GetComponent<Box>();
                if (box.owner != null && box.owner.isMine)
                    box.rigidbody.AddExplosionForce(-grawforce, cur2.position, gravdist);
            }
    }
    private void UpdateOwners()
    {

        foreach (GameObject a in GameObject.FindGameObjectsWithTag("Box"))
            if (Input.GetMouseButtonDown(1) && Vector3.Distance(a.transform.position, cur2.position) < gravdist)
                foreach (NetworkView b in a.GetComponents<NetworkView>())
                    if (b.isMine)
                    {
                        Network.RemoveRPCs(Network.player, (int)Group.SetOwner);
                        b.group = (int)Group.SetOwner;
                        b.RPC("SetOwner", RPCMode.AllBuffered);
                    }
    }




}





