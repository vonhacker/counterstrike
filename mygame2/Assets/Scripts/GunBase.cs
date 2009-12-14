using UnityEngine;
using System.Collections;
public class GunBase : Base
{
    public float bullets=9999;
    public virtual void DisableGun()
    {        
        Show(false);
    }
    protected override void Start()
    {
        
    }
    Quaternion q;
    protected override void FixedUpdate()
    {
        if (isMine) q = Find<Cam>().transform.rotation;
        this.transform.rotation = q;
    }

    protected override void Update()
    {
        if (isMine) q = Find<Cam>().transform.rotation;
        transform.rotation = q;        
    }
    protected override void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (isMine) q = Find<Cam>().transform.rotation;
        stream.Serialize(ref q);
        transform.rotation = q;        
    }
    
    
    public virtual void EnableGun()
    {        
        Show(true);
    }
}