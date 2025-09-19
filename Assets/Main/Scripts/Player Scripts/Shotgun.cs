using UnityEngine;

public class Shotgun : Gun
{
    public override void HandleShooting()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && _currentAmmo > 0)
        {
            Shoot();
        //    Debug.Log("<color=green>" + name + "</color> Tirando");
        }
    }

    public override void HandleReloading()
    {
        if (_maxAmmo > 0 && Input.GetKeyDown(KeyCode.R))
        {
            Reload();
      //      Debug.Log("<color=green>" + name + "</color> Recargando");
        }
    }
}
