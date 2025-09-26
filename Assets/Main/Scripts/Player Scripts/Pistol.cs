using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : Gun
{
    public override void HandleShooting()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Shoot();
        //    Debug.Log("<color=green>" + name + "</color> Tirando");
        }
    }

    public override void HandleReloading()
    {
        if (MaxAmmo > 0 && Input.GetKeyDown(KeyCode.R))
        {
            Reload();
      //      Debug.Log("<color=green>" + name + "</color> Recargando");
        }
    }
}
