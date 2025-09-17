using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class ZombieController : MonoBehaviourPunCallbacks, IDamageable
{
    private NavMeshAgent _agent;
    private HealthScript health;
    [SerializeField] private EnemyStats _stats;
    private ZombieView zombieView;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        health = GetComponent<HealthScript>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
    }

    private void Start()
    {
        if (!photonView.IsMine) return;

        _agent.speed = _stats._speed;
        health.InitHealth(_stats._health);
    }

    public void GetDamage(int damage)
    {
        health.TakeDamage(damage);
    }
}
