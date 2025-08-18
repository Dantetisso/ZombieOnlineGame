using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerGunSync : MonoBehaviourPun // este script es para que se puedan dar los rpc de las armas, es como un puente entre el gunholder y photon
{                                               // asi no se tiene que poner mas photon view a las armas, solo usa el photonview del player
    [SerializeField] private GunHolderScript gunHolder;

    [PunRPC]
    public void RPC_ShowMuzzleFlash()
    {
        var activeGun = gunHolder.GetActiveGun();

        if (activeGun != null)
        {
            activeGun.ShowFlash();// si tiene un arma llama a la corrutina del flash
        }
    }

    [PunRPC]
    public void RPC_PlayShootSound()
    {
        var activeGun = gunHolder.GetActiveGun();

        if (activeGun != null) // lo mismo que el muzzle pero reproduce sonido
        {
            activeGun.PlayShootSound();
        }
    }

    [PunRPC]
    public void RPC_ChangeGun(int id)
    {
        gunHolder.ChangeGun(id);
    }

    [PunRPC]
    public void RPC_MakeDamage(int targetViewID, int damage) // busco si el objeto al que le pegue tiene la interfaz IDamageable y llama al metodo GetDamage para dañarlo
    {
        PhotonView targetPhotonView = PhotonView.Find(targetViewID);

        Debug.Log($"RPC de hacer daño en: <color=cyan>{targetViewID}</color> con un daño de: <color=yellow>{damage}</color>"); // debug para chequear a que le hace daño y cuanto

        if (targetPhotonView != null && targetPhotonView.TryGetComponent(out IDamageable damageable))
        {
            damageable.GetDamage(damage);
        }
    }
    
}
