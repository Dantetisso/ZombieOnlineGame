using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class ZombieController : MonoBehaviourPunCallbacks, IDamageable
{
    [Header("References")]
    private NavMeshAgent navAgent;
    private HealthScript health;
    private LineOfSightMono lineOfSight;

    [SerializeField] private EnemyStats enemyStats;
    [SerializeField] private Transform attackPoint; // Punto central para el overlap de ataque

    [Header("Patrol")]
    private Transform[] patrolWaypoints;
    private int currentWaypointIndex = 0;
    private bool canPatrol = true;

    [Header("AI State")]
    private EnemyStates currentState;
    private float lastAttackTime;

    private PlayerMovement[] allPlayers;

    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        health = GetComponent<HealthScript>();
        lineOfSight = GetComponent<LineOfSightMono>();

        navAgent.updateRotation = false;
        navAgent.updateUpAxis = false;
    }

    private void Start()
    {
        navAgent.speed = enemyStats._speed;
        lineOfSight.range = enemyStats._viewRange;
        health.InitHealth(enemyStats._health);

        allPlayers = FindObjectsOfType<PlayerMovement>();

        if (canPatrol && patrolWaypoints != null && patrolWaypoints.Length > 0)
        {
            currentWaypointIndex = 0;
            navAgent.SetDestination(patrolWaypoints[currentWaypointIndex].position);
            currentState = EnemyStates.Patrol;
        }
    }

    private void Update()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;

        switch (currentState)
        {
            case EnemyStates.Patrol: PatrolBehavior(); break;
            case EnemyStates.Chase: ChaseBehavior(); break;
            case EnemyStates.Attack: AttackBehavior(); break;
        }
    }

    #region Patrol
    private void PatrolBehavior()
    {
        Transform detectedPlayer = DetectPlayerInLOS();
        if (detectedPlayer != null)
        {
            currentState = EnemyStates.Chase;
            return;
        }

        if (patrolWaypoints.Length == 0 || !canPatrol) return;

        if (!navAgent.pathPending && navAgent.remainingDistance < 0.2f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
            navAgent.SetDestination(patrolWaypoints[currentWaypointIndex].position);
        }
    }
    #endregion

    #region Chase
    private void ChaseBehavior()
    {
        Transform detectedPlayer = DetectPlayerInLOS();
        if (detectedPlayer == null)
        {
            currentState = canPatrol ? EnemyStates.Patrol : EnemyStates.Idle;
            navAgent.isStopped = false;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, detectedPlayer.position);

        if (distanceToPlayer > enemyStats._attackDistance)
        {
            navAgent.isStopped = false;
            navAgent.SetDestination(detectedPlayer.position);
        }
        else
        {
            navAgent.isStopped = true;
            currentState = EnemyStates.Attack;
        }
    }
    #endregion

    #region Attack
    private void AttackBehavior()
    {
        Collider2D[] playersInRange = Physics2D.OverlapCircleAll(
            attackPoint.position,
            enemyStats._attackDistance,
            enemyStats._attackLayer
        );

        Transform targetPlayer = null;

        foreach (var col in playersInRange)
        {
            if (col.TryGetComponent<PlayerMovement>(out PlayerMovement player))
            {
                if (lineOfSight.LOS(player.transform))
                {
                    targetPlayer = player.transform;
                    break;
                }
            }
        }

        if (targetPlayer == null)
        {
            currentState = canPatrol ? EnemyStates.Patrol : EnemyStates.Idle;
            navAgent.isStopped = false;
            return;
        }

        // Ataque con cooldown
        if (Time.time - lastAttackTime >= enemyStats._attackDuration)
        {
            lastAttackTime = Time.time;
            DealDamageToTarget(targetPlayer);
        }
    }
    #endregion

    private Transform DetectPlayerInLOS()
    {
        foreach (var player in allPlayers)
        {
            if (player != null && lineOfSight.LOS(player.transform))
                return player.transform;
        }
        return null;
    }

    private void DealDamageToTarget(Transform target)
    {
        PhotonView targetView = target.GetComponent<PhotonView>();
        PhotonView myView = transform.root.GetComponent<PhotonView>();

        if (PhotonNetwork.IsConnected)
        {
            if (targetView && myView)
                myView.RPC(nameof(RPC_DealDamage), RpcTarget.All, targetView.ViewID, enemyStats._damage);
        }
        else
        {
            if (target.TryGetComponent(out IDamageable dmg))
                dmg.GetDamage(enemyStats._damage);
        }
    }

    public void SetWaypoints(Transform[] waypoints)
    {
        patrolWaypoints = waypoints;
        currentWaypointIndex = 0;

        if (canPatrol && patrolWaypoints.Length > 0)
        {
            navAgent.SetDestination(patrolWaypoints[currentWaypointIndex].position);
            currentState = EnemyStates.Patrol;
        }
    }

    public void DisablePatrol()
    {
        canPatrol = false;
        currentState = EnemyStates.Idle;
    }

    [PunRPC]
    public void RPC_DealDamage(int targetViewID, int damage)
    {
        PhotonView targetPhotonView = PhotonView.Find(targetViewID);
        if (targetPhotonView != null && targetPhotonView.TryGetComponent(out IDamageable damageable))
        {
            damageable.GetDamage(damage);
        }
    }

    public void GetDamage(int damage)
    {
        health.TakeDamage(damage);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, enemyStats._attackDistance);
        }
    }
}
