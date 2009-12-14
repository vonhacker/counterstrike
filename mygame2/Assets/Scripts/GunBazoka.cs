using UnityEngine;
using System.Collections;

public class GunBazoka : GunBase
{
    
    
    float lt;
    protected override void Update()
    {
        base.Update();
        if(isMine)
            LocalUpdate();
    }
    
    private void LocalUpdate()
    {
        if (Time.time - lt > 1 && Input.GetMouseButton(0))
            LocalShoot();
        
    }
    public Transform _Patron;
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }
    Cam cam { get { return Find<Cam>(); } }
    public LayerMask Default;
    public void LocalShoot()
    {
        if (!Screen.lockCursor || bullets <= 0) return;
        bullets--;
        lt = Time.time;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));//new Ray(cam.transform.position, cam.transform.TransformDirection(Vector3.forward));  
        ray.origin = ray.GetPoint(10);
        RaycastHit h;
        Transform t = transform.Find("cursor");
        if (Physics.Raycast(ray, out h, float.MaxValue, 1))
            t.LookAt(h.point);
        else
            t.rotation = cam.transform.rotation;

        RPCShoot(t.position, t.rotation);        
    }

    [RPC]
    private void RPCShoot(Vector3 pos,Quaternion rot)
    {        
        CallRPC(pos, rot);
        ((Transform)Instantiate(_Patron, pos, rot)).GetComponent<Base>().OwnerID = OwnerID;
    }

}