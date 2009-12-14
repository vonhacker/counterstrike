using UnityEngine;
using System.Collections;

public class GunBazoka : GunBase
{
    public void EnableBazoka()
    {

        
        enabled = true;
    }
    float lt;
    protected override void Update()
    {
        if(isMine)
            LocalUpdate();
    }

    private void LocalUpdate()
    {
        if (Time.time - lt > 1 && Input.GetMouseButton(0))
            Shoot();
        
    }
    public Transform _Patron;
    protected override void FixedUpdate()
    {
        
    }
    Cam cam { get { return Find<Cam>(); } }
    public LayerMask Default;
    public void Shoot()
    {
        if (!Screen.lockCursor) return;
        lt = Time.time;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));//new Ray(cam.transform.position, cam.transform.TransformDirection(Vector3.forward));  
        ray.origin = ray.GetPoint(10);
        RaycastHit h;
        if (Physics.Raycast(ray, out h, float.MaxValue, 1))
            transform.LookAt(h.point);
        else
            transform.rotation = cam.transform.rotation;

        RPCShoot(transform.position, transform.rotation);        
    }

    [RPC]
    private void RPCShoot(Vector3 pos,Quaternion rot)
    {        
        CallRPC(pos, rot);
        ((Transform)Instantiate(_Patron, pos, rot)).GetComponent<Base>().OwnerID = OwnerID;
    }

}