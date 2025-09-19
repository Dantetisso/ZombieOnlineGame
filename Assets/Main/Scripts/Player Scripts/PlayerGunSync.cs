using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerGunSync : MonoBehaviourPun // este script es para que se puedan dar los rpc de las armas, es como un puente entre el gunholder y photon
{                                               // asi no se tiene que poner mas photon view a las armas, solo usa el photonview del player

    // Este método busca el arma activa (GameObject activo con componente Gun)
    private Gun GetActiveGun()
    {
        Gun[] guns = GetComponentsInChildren<Gun>(true); // busca todas incluso inactivas
        foreach (Gun gun in guns)
        {
            if (gun.gameObject.activeInHierarchy)
                return gun;
        }
        return null;
    }

    [PunRPC]
    public void RPC_ShowMuzzleFlash()
    {
        var activeGun = GetActiveGun();
        if (activeGun != null)
            activeGun.ShowFlash();
    }

    [PunRPC]
    public void RPC_PlayShootSound()
    {
        var activeGun = GetActiveGun();
        if (activeGun != null)
            activeGun.PlayShootSound();
    }

    [PunRPC]
    public void RPC_MakeDamage(int targetViewID, int damage)
    {
        PhotonView targetPhotonView = PhotonView.Find(targetViewID);

        if (targetPhotonView != null && targetPhotonView.TryGetComponent(out IDamageable damageable))
        {
            damageable.GetDamage(damage);
        }
    }
}
