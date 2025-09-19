using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rifle : Gun
{
    private float _nextFireTime;

    public override void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Mouse0) && Time.time >= _nextFireTime && currentAmmo > 0)
        {
            _nextFireTime = Time.time + 1f / gunData._fireFate;
            Shoot();
            Debug.Log("Rifle TIRANDO");
        }
    }

    public override void HandleReloading()
    {
        if (maxAmmo > 0 && Input.GetKeyDown(KeyCode.R))
        {
            Reload();
            Debug.Log("Recarga Rifle");
        } 
    }
}
