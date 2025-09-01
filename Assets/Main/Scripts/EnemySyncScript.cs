using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemySyncScript : MonoBehaviour
{
    [PunRPC]
    public void RPC_EnemyDamage(int targetViewID, int damage) // busco si el objeto al que le pegue tiene la interfaz IDamageable y llama al metodo GetDamage para dañarlo
    {
        PhotonView targetPhotonView = PhotonView.Find(targetViewID);

        Debug.Log($"RPC, ENEMIGO de hacer daño en: <color=cyan>{targetViewID}</color> con un daño de: <color=yellow>{damage}</color>"); // debug para chequear a que le hace daño y cuanto

        if (targetPhotonView != null && targetPhotonView.TryGetComponent(out IPlayer damageable))
        {
            damageable.GetDamage(damage);
            Debug.Log("Recibio dalo mio");
        }
    }
}
