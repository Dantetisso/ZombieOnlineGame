using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class ZombieController : MonoBehaviourPunCallbacks, IDamageable
{
    [Header("References")]
    public NavMeshAgent navAgent; // público para acceso desde spawner
    public HealthScript health;
    public LineOfSightMono lineOfSight;
    public EnemyStats enemyStats; // público para acceso desde spawner

    [Header("Patrol Settings")]
    private Transform[] patrolWaypoints;
    private int currentWaypointIndex = 0;
    private bool canPatrol = false;

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
        else
        {
            currentState = EnemyStates.Idle;
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
        Transform targetPlayer = DetectPlayerInLOS();
        if (targetPlayer != null)
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
        Transform targetPlayer = DetectPlayerInLOS();
        if (targetPlayer == null)
        {
            currentState = canPatrol ? EnemyStates.Patrol : EnemyStates.Idle;
            navAgent.isStopped = false;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

        if (distanceToPlayer > enemyStats._attackRange)
        {
            navAgent.isStopped = false;
            navAgent.SetDestination(targetPlayer.position);
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
            transform.position,
            enemyStats._attackRange,
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
        if (Time.time - lastAttackTime >= enemyStats._attackSpeed)
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
        PhotonView targetPhotonView = target.GetComponent<PhotonView>();
        PhotonView myView = transform.root.GetComponent<PhotonView>();

        if (PhotonNetwork.IsConnected)
        {
            if (targetPhotonView && myView)
                myView.RPC(nameof(RPC_DealDamage), RpcTarget.All, targetPhotonView.ViewID, enemyStats._damage);
        }
        else
        {
            if (target.TryGetComponent(out IDamageable dmg))
                dmg.TakeDamage(enemyStats._damage);
        }
    }

    public void SetPatrolWaypoints(Transform[] waypoints)
    {
        patrolWaypoints = waypoints;
        currentWaypointIndex = 0;

        if (patrolWaypoints.Length > 0)
        {
            canPatrol = true;
            navAgent.SetDestination(patrolWaypoints[currentWaypointIndex].position);
            currentState = EnemyStates.Patrol;
        }
        else
        {
            canPatrol = false;
            currentState = EnemyStates.Idle;
        }
    }

    public void EnablePatrol()
    {
        canPatrol = true;
        if (patrolWaypoints.Length > 0)
        {
            currentWaypointIndex = 0;
            navAgent.SetDestination(patrolWaypoints[currentWaypointIndex].position);
            currentState = EnemyStates.Patrol;
        }
    }

    [PunRPC]
    public void RPC_DealDamage(int targetViewID, int damage)
    {
        PhotonView targetPhotonView = PhotonView.Find(targetViewID);
        if (targetPhotonView != null && targetPhotonView.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(damage);
        }
    }

    public void TakeDamage(int damage)
    {
        health.TakeDamage(damage);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyStats._attackRange);
    }
}
