using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class GunHolderScript : MonoBehaviourPun
{
    [SerializeField] GameObject[] guns;
    
    public void ChangeGun(int id)
    {
        for (int i = 0; i < guns.Length; i++)
        {
            if (guns[i] != null)
            {
                guns[i].SetActive(i == id);
            }
        }
    }

    public void LoadGun(int id)
    {
        for (int i = 0; i < guns.Length; i++)
        {
            if (guns[i] != null)
            {
                var gi = guns[i].GetComponent<Gun>();
                gi.FullReload();
            }
        }
    }

    public Gun GetActiveGun() // recorro el array para saber cual esta activa y devuelvo el componente gun
    {
        foreach (var gun in guns)
        {
            if (gun.activeInHierarchy)
            {
                return gun.GetComponent<Gun>();
            }
        }
        return null;
    }

}