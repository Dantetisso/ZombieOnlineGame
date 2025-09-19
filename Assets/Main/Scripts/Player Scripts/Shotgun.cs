using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Gun
{
    public override void HandleShooting()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && currentAmmo > 0)
        {
            Shoot();
            Debug.Log("<color=green>" + this.name + "</color>" + " Tirando");
        }
    }

    public override void HandleReloading()
    {
        if (maxAmmo > 0 && Input.GetKeyDown(KeyCode.R))
        {
            Reload();
            Debug.Log("<color=green>" + this.name + "</color>" + " Recargando");
        } 
    }

}
