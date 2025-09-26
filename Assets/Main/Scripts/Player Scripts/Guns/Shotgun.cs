using UnityEngine;
using Photon.Pun;

public class Shotgun : Gun
{
    public override void Shoot()
    {
        if (IsReloading) return;

        if (Time.time < nextFireTime) return; // respeta cadencia del arma

        CurrentAmmo--;
        NotifyAmmoChange();

        nextFireTime = Time.time + gunData._fireFate;

        int pellets = Mathf.Max(1, gunData._pellets);
        float spreadAngle = gunData._spreadAngle;

        float damagePerPellet = (float)gunData._damage / pellets;

        Vector2 origin = shootPoint.position;
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dirToMouse = (mouseWorld - origin).normalized;

        float startSpreadAngle = -spreadAngle / 2f;
        float angleStep = spreadAngle / (pellets - 1);

        for (int i = 0; i < pellets; i++)
        {
            float pelletAngle = startSpreadAngle + angleStep * i;
            Vector2 pelletDirection = Quaternion.Euler(0, 0, pelletAngle) * dirToMouse;

            float shotRange = Mathf.Min(Vector2.Distance(origin, mouseWorld), gunData._range);

            RaycastHit2D hit = Physics2D.Raycast(origin, pelletDirection, shotRange, zombieMask);

            if (hit.collider && hit.distance <= shotRange)
            {
                PhotonView targetPhotonView = hit.collider.GetComponent<PhotonView>();

                if (PhotonNetwork.IsConnected)
                {
                    if (playerPhotonView != null && targetPhotonView != null)
                    {
                        playerPhotonView.RPC(nameof(PlayerGunSync.RPC_MakeDamage), RpcTarget.MasterClient, targetPhotonView.ViewID, (int)damagePerPellet);
                    }
                }
                else
                {
                    if (hit.collider.TryGetComponent(out HealthScript enemyHealth))
                    {
                        enemyHealth.TakeDamage((int)damagePerPellet);
                    }
                }
            }

            Debug.DrawRay(origin, pelletDirection * shotRange, Color.yellow, 0.5f);
        }

        PlayShootSound();
        ShowFlash();

        if (PhotonNetwork.IsConnected && playerPhotonView != null)
        {
            playerPhotonView.RPC(nameof(PlayerGunSync.RPC_PlayShootSound), RpcTarget.Others);
            playerPhotonView.RPC(nameof(PlayerGunSync.RPC_ShowMuzzleFlash), RpcTarget.Others);
        }
    }
}
