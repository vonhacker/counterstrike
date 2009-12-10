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
        if (isMine) q = Find<Cam>().transform.rotation;
        this.transform.rotation = q;
        if (isMine)
            LocalUpdate();

    }

    private void LocalUpdate()
    {
        SetMouseDown(Input.GetMouseButton(1));

        LocalUpdatePowerGun();
        if (MouseDown1 && weapon == Weapon.AntiGrawitaty)
            foreach (GameObject a in GameObject.FindGameObjectsWithTag("Box"))
                a.rigidbody.AddExplosionForce(-grawforce, cur2.position, gravdist);
    }
    private void LocalUpdatePowerGun()
    {
        GameObject[] cts = GameObject.FindGameObjectsWithTag("Box");
        foreach(GameObject a in GameObject.FindGameObjectsWithTag("Box"))
        {            
            if (MouseDown1 && Vector3.Distance(a.transform.position, cur2.position) < gravdist)
                LocalSetEnabled(a, true);
            else
                LocalSetEnabled(a, false);
        }
    }

    private void LocalSetEnabled(GameObject a, bool value)
    {

        foreach (NetworkView b in a.GetComponents<NetworkView>())
            if (b.isMine)
            {
                if (b.enabled != value)
                {
                    b.enabled = value;
                    if (value)
                    {
                        a.GetComponent<Box>().SetOwner(Network.player);
                        b.RPC("SetScopeTrue", RPCMode.Server);//server not included
                    }
                    else
                        b.RPC("SetScopeFalse", RPCMode.Server);//server not included
                }
            }
    }


}



//[RPC]
//void SetEnabled2(NetworkViewID id, bool value)
//{
//    Trace.Log("<<<<<Set" + id + value);
//    Call("SetEnabled2", id, value);
//    foreach (GameObject a in GameObject.FindGameObjectsWithTag("Box"))
//        foreach (NetworkView b in a.GetComponents<NetworkView>())
//            if (b.viewID == id) { b.enabled = value; return; }

//    Trace.Log("<<<<<<<<<<<EEEERRRRRRRRRRRRRRRORRRR>>>>>>>>>>>>"+id);

//}
//[RPC]
//void SetEnabled2(NetworkViewID id, bool value)
//{
//    Trace.Log("<<<<<Set" + id + value);
//    if (IsMine && !Network.isServer)
//        networkView.RPC("SetEnabled2", RPCMode.Server, id, value);

//    ////nw.enabled = false;

//    foreach (NetworkPlayer np in Network.connections)
//        NetworkView.Find(id).SetScope(np, value);
//}

//private void SetEnabled(GameObject a, bool enbled)
//{             
//    foreach (NetworkView b in a.GetComponents<NetworkView>())
//        if (b.isMine)
//            enable(b.viewID, enbled);
//}
//[RPC]
//void enable(NetworkViewID id,bool value)
//{
//    if (NetworkView.Find(id).enabled != value)
//    {
//        CallLast(Group.Rig, "enable",id, value);
//        NetworkView.Find(id).enabled = value;
//    }
//}



//g.GetComponent<Base>().OwnerID = OwnerID;        
//g.networkView.observed = g.rigidbody;            

