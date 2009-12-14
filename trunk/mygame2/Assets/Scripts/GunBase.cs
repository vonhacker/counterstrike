using UnityEngine;
using System.Collections;
public class GunBase : Base
{
    public virtual void DisableGun()
    {
        enabled = false;
    }

    public virtual void EnableGun()
    {
        enabled = true;
    }
}