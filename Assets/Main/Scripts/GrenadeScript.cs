using UnityEngine;
using Photon.Pun;

public class GrenadeScript : MonoBehaviourPunCallbacks
{
    [SerializeField] private float explosionDelay;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int grenadeDamage;
    [SerializeField] private float grenadeRange;

    private void Start()
    {
        Invoke(nameof(Explode), explosionDelay);
    }

    void Explode()
    {
        PhotonNetwork.Instantiate(explosionEffect.name, transform.position, Quaternion.identity);

        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, new Vector2(grenadeRange, grenadeRange), 0f, enemyLayer);
        
        foreach (var coll in colliders)
        {
            if (coll == null) continue;

            if (PhotonNetwork.IsConnected && photonView.IsMine)
            {
                var enemy = coll.GetComponent<PhotonView>();

                if (enemy != null)
                {
                    photonView.RPC(nameof(RPC_GrenadeAttack), RpcTarget.MasterClient, enemy.ViewID, grenadeDamage);
                }
            }
            else
            {
                if (coll.TryGetComponent(out IDamageable dmg))
                {
                    dmg.TakeDamage(grenadeDamage);
                }
            }
        }
        
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [PunRPC]
    public void RPC_GrenadeAttack(int enemyViewID, int dmg)
    {
        PhotonView enemyPhoton = PhotonView.Find(enemyViewID);
        if (enemyPhoton != null && enemyPhoton.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(dmg);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, grenadeRange);
    }
}
