using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rifle : Gun
{
    public override void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Mouse0) && Time.time >= nextFireTime && currentAmmo > 0)
        {
            nextFireTime = Time.time + 1f / gunData._fireFate;
            Shoot();
            Debug.Log("<color=yellow>" + this.name + "</color>" + " Tirando");
        }
    }

    public override void HandleReloading()
    {
        if (maxAmmo > 0 && Input.GetKeyDown(KeyCode.R))
        {
            Reload();
            Debug.Log("<color=yellow>" + this.name + "</color>" + " Recargando");
        } 
    }
}
